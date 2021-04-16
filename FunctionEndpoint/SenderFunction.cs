using System.IO;
using System.Threading.Tasks;
using AzureFunctionsTakeTheBus.FunctionEndpoint.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;

namespace AzureFunctionsTakeTheBus.FunctionEndpoint
{
    public class SenderFunction
    {
        readonly IFunctionEndpoint endpoint;
        
        public SenderFunction(IFunctionEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }
        
        [FunctionName("Sender")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "send")]
            HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            var name = data?.name;

            //var sendOptions = new SendOptions{};
            //await endpoint.Send(new AMessage($"Ciao {name} from A!"), sendOptions, executionContext, log);

            var publishOptions = new PublishOptions();
            await endpoint.Publish(new AMessage($"Ciao {name} from A!"), publishOptions, executionContext, log);

            return new AcceptedResult();
        }
    }
}