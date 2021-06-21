using System;
using System.Threading.Tasks;
using AzureFunctionsTakeTheBus.Shared.Messages;
using Bogus;
using NServiceBus;
using NServiceBus.Logging;

namespace AzureFunctionsTakeTheBus.FunctionEndpoint.Handlers
{
    public class AssembleBikeMessageHandler : IHandleMessages<AssembleBikeMessage>
    {
        static readonly ILog log = LogManager.GetLogger<AssembleBikeMessageHandler>();
        static readonly Faker faker = new();
        
        public AssembleBikeMessageHandler()
        {
            
        }
        
        public async Task Handle(AssembleBikeMessage message, IMessageHandlerContext context)
        {
            log.Warn($"Handling {nameof(AssembleBikeMessage)} in {nameof(AssembleBikeMessageHandler)}.");
            await Task.Delay(TimeSpan.FromSeconds(faker.Random.Number(1,5)));
            
            //var sendOptions = new SendOptions();
            //sendOptions.SetDestination("fantastic-bike-shipper");
            //await context.Send(new ShipBikeMessage(message.Id, faker.Address.FullAddress()), sendOptions);
            
            await context.SendLocal(new ShipBikeMessage(message.Id, faker.Address.FullAddress()));
            log.Warn($"Bike {message.Id} assembled and ready to be shipped!");
        }
    }
}
