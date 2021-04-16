using Elfo.NsbFunctions.FunctionEndpoint;
using Elfo.NsbFunctions.FunctionEndpoint.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using NServiceBus;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Elfo.NsbFunctions.FunctionEndpoint
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services
                .AddRoundWriteCycle(configuration)
                .AddRequiredServices(configuration)
                .ConfigureLogger(configuration);

            //create topology, sync for now
            NServiceBusExtensions.CreateTopology(configuration, 
                    auditQueue: configuration["NServiceBus:AuditQueue"], 
                    errorQueue: configuration["NServiceBus:ErrorQueue"])
                .GetAwaiter().GetResult();
            
            builder.UseNServiceBus(() => NServiceBusExtensions.BuildEndpointConfiguration(builder, configuration));
        }
    }
}
