using System;
using System.Threading.Tasks;
using AzureFunctionsTakeTheBus.Shared.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace AzureFunctionsTakeTheBus.FunctionEndpoint.Handlers
{
    public class ShipBikeMessageHandler : IHandleMessages<ShipBikeMessage>
    {
        static readonly ILog log = LogManager.GetLogger<ShipBikeMessageHandler>();
        static readonly Random random = new();
        
        public ShipBikeMessageHandler()
        {
            
        }
        
        public async Task Handle(ShipBikeMessage message, IMessageHandlerContext context)
        {
            log.Warn($"Handling {nameof(ShipBikeMessage)} in {nameof(ShipBikeMessageHandler)}.");
            await Task.Delay(TimeSpan.FromSeconds(random.Next(1,5)));
            log.Warn($"Bike {message.Id} shipped to {message.Address}!");
            
            //throw new Exception("AZZZ!!!");
        }
    }
}
