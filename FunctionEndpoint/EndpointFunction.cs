using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsTakeTheBus.FunctionEndpoint
{
    public class EndpointFunction
    {
        const string EndpointName = "functionstakebus-endpoint";
        readonly NServiceBus.FunctionEndpoint endpoint;
        public EndpointFunction(NServiceBus.FunctionEndpoint endpoint) => this.endpoint = endpoint;

        [FunctionName(EndpointName)]
        public Task Run(
            [ServiceBusTrigger(queueName: "%NServiceBus:EndpointName%")] Message message, 
            ILogger logger, 
            ExecutionContext executionContext) =>
            endpoint.Process(message, executionContext, logger);
    }
}
