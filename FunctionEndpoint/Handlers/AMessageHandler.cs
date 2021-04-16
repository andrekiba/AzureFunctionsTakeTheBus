using System.Threading.Tasks;
using AzureFunctionsTakeTheBus.FunctionEndpoint.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace AzureFunctionsTakeTheBus.FunctionEndpoint
{
    public class AMessageHandler : IHandleMessages<AMessage>
    {
        static readonly ILog log = LogManager.GetLogger<AMessageHandler>();
        
        public AMessageHandler()
        {
            
        }
        
        public Task Handle(AMessage message, IMessageHandlerContext context)
        {
            log.Info($"Handling {nameof(AMessage)} in {nameof(AMessageHandler)}.");
            log.Info(message.Payload);
            
            return Task.CompletedTask;
        }
    }
}
