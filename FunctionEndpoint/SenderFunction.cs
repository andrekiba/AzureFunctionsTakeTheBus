using System;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsTakeTheBus.Shared.Messages;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;

namespace AzureFunctionsTakeTheBus.FunctionEndpoint
{
    public class SenderFunction
    {
        readonly IFunctionEndpoint endpoint;
        readonly QueueClient assemblerQueue;
        
        public SenderFunction(IFunctionEndpoint endpoint, IConfiguration configuration)
        {
            this.endpoint = endpoint;
            assemblerQueue = new QueueClient(
                configuration.GetValue<string>("AzureWebJobsServiceBus"),
                configuration.GetValue<string>("NServiceBus:EndpointName"));
        }
        
        [FunctionName("Sender")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "buy")]
            HttpRequest req,
            ILogger logger,
            ExecutionContext executionContext)
        {
            var bike = ProduceBike();
            var bikeMessage = new AssembleBikeMessage(bike.Id, bike.Price, bike.Model, bike.Parts);
            
            #region Native
            /*
            var rowBikeMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bikeMessage));
            var nativeMessage = new Message(rowBikeMessage)
            {
                MessageId = Guid.NewGuid().ToString(),
                UserProperties =
                {
                    {"NServiceBus.EnclosedMessageTypes", typeof(AssembleBikeMessage).FullName}
                }
            };
                
            logger.LogWarning($"Sending buying request for bike {bike.Id}");
            await assemblerQueue.SendAsync(nativeMessage).ConfigureAwait(false);
            logger.LogWarning($"Bike {bike.Id} bought successfully!");
            */
            #endregion
            
            #region NServiceBus
            
            logger.LogWarning($"Sending buying request for bike {bike.Id}");

            var sendOptions = new SendOptions();
            sendOptions.RouteToThisEndpoint();
            await endpoint.Send(bikeMessage, sendOptions, executionContext, logger);
            
            //var publishOptions = new PublishOptions();
            //await endpoint.Publish(bikeMessage, publishOptions, executionContext, logger);
            
            logger.LogWarning($"Bike {bike.Id} bought successfully!");
            
            #endregion 
            
            return new AcceptedResult();
        }
        
        static Bike ProduceBike()
        {
            string[] bikePartNames =
            {
                "wheel", "rim", "tire", "brake", "seat", "cassette", "rear-derailleur", "front-derailleur",  
                "chain", "chainring", "crankset", "pedal", "headset", "stem", "handlerbar", "fork", "frame",
                "hub", "bottle-cage", "disk"
            };
            string[] bikeModels = 
            { 
                "mtb-xc", "mtb-trail", "mtb-enduro", "mtb-downhill", "bdc-aero",
                "bdc-endurance", "gravel", "ciclocross", "trekking", "urban" 
            };
            
            var bikePartGen = new Faker<BikePart>()
                .RuleFor(x => x.Id, () => Guid.NewGuid().ToString())
                .RuleFor(x => x.Name, f => f.PickRandom(bikePartNames))
                .RuleFor(x => x.Code, f => f.Commerce.Ean8());

            var bikeGen = new Faker<Bike>()
                .RuleFor(x => x.Id, () => Guid.NewGuid().ToString())
                .RuleFor(x => x.Price, f => f.Random.Number(200,10000))
                .RuleFor(x => x.Model, f => f.PickRandom(bikeModels))
                .RuleFor(u => u.Parts, f => bikePartGen.Generate(f.Random.Number(6,bikePartNames.Length)));
            
            return bikeGen.Generate();
        }
    }
}