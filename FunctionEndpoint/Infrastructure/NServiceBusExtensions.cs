using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace Elfo.NsbFunctions.FunctionEndpoint.Infrastructure
{
    public static class NServiceBusExtensions
    {
        public static ServiceBusTriggeredEndpointConfiguration BuildEndpointConfiguration(IFunctionsHostBuilder builder, IConfiguration configuration)
        {
            var endpointConfiguration = new ServiceBusTriggeredEndpointConfiguration(configuration["NServiceBus:EndpointName"]);
            endpointConfiguration.LogDiagnostics();
            var e = endpointConfiguration.AdvancedConfiguration;

            //var transport = e.UseTransport<AzureServiceBusTransport>();
            //transport.ConnectionString(configuration["AzureWebJobsServiceBus"]);

            var persistence = e.UsePersistence<SqlPersistence>();
            persistence.ConnectionBuilder(() => new SqlConnection(configuration.GetConnectionString("Db")));
            persistence.TablePrefix(string.Empty);
            var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
            dialect.Schema("nservicebus");
            var sagaSettings = persistence.SagaSettings();
            sagaSettings.JsonSettings(EventSerialization.Settings);

            //var outboxSettings = e.EnableOutbox();
            //outboxSettings.UseTransactionScope();
            //outboxSettings.KeepDeduplicationDataFor(TimeSpan.FromDays(6));
            //outboxSettings.RunDeduplicationDataCleanupEvery(TimeSpan.FromMinutes(15));

            var serialization = e.UseSerialization<NewtonsoftSerializer>();
            // Newtonsoft serializer doesn't properly deserialize properties with protected setter,
            // the following serializer settings will be applied by NServiceBus during all messages's deserializations
            serialization.Settings(EventSerialization.Settings);

            e.Conventions().DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));

            e.AuditProcessedMessagesTo(configuration["NServiceBus:AuditQueue"]);
            e.SendFailedMessagesTo(configuration["NServiceBus:ErrorQueue"]);
            //e.SendHeartbeatTo(serviceControlQueue: configuration["NServiceBus:ServiceControlInstance"],
            //    frequency: TimeSpan.FromSeconds(15),
            //    timeToLive: TimeSpan.FromSeconds(30));

            //var endpointName = configuration["NServiceBus:EndpointName"];
            //var machineName = $"{Dns.GetHostName()}.{IPGlobalProperties.GetIPGlobalProperties().DomainName}";
            //var instanceIdentifier = $"{endpointName}@{machineName}";

            //var metrics = e.EnableMetrics();
            //metrics.SendMetricDataToServiceControl(serviceControlMetricsAddress: configuration["NServiceBus:ServiceControlMonitoringInstance"],
            //    interval: TimeSpan.FromSeconds(10),
            //    instanceId: instanceIdentifier);

            //For Development ONLY you can install NServiceBus.SagaAudit package and activate this monitoring feature
            //https://docs.particular.net/nservicebus/sagas/saga-audit
            //e.AuditSagaStateChanges(serviceControlQueue: configuration["NServiceBus:ServiceControlInstance"]);

            //For Data Annotation validations you can activate the NServiceBus.DataAnnotations unofficial extension
            //https://www.nuget.org/packages/NServiceBus.DataAnnotations/
            //e.UseDataAnnotationsValidation();

            //This instruction checks every time the application starts up in order to create
            //all the necessary NServiceBus objects in the database automatically
            var settings = e.GetSettings();
            settings.Set("SqlPersistence.ScriptDirectory", builder.GetContext().ApplicationRootPath);
            e.EnableInstallers();

            return endpointConfiguration;
        }
        
        public static async Task CreateTopology(IConfiguration configuration, 
            string topicName = "bundle-1", 
            string auditQueue = "audit", 
            string errorQueue = "error")
        {
            var connectionString = configuration.GetValue<string>("AzureWebJobsServiceBus");
            var managementClient = new ManagementClient(connectionString);

            var attribute = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttribute<FunctionNameAttribute>(false) != null)
                .SelectMany(m => m.GetParameters())
                .SelectMany(p => p.GetCustomAttributes<ServiceBusTriggerAttribute>(false))
                .FirstOrDefault();

            if (attribute == null)
                throw new Exception("No endpoint was found");
            
            //var endpointQueueName = attribute.QueueName
            var endpointQueueName = configuration["NServiceBus:EndpointName"];
            
            //create the queue
            if (!await managementClient.QueueExistsAsync(endpointQueueName))
                await managementClient.CreateQueueAsync(endpointQueueName);
            
            //create the topic
            if (!await managementClient.TopicExistsAsync(topicName))
                await managementClient.CreateTopicAsync(topicName);

            //subscribe to the topic
            if (!await managementClient.SubscriptionExistsAsync(topicName, endpointQueueName))
            {
                var subscriptionDescription = new SubscriptionDescription(topicName, endpointQueueName)
                {
                    ForwardTo = endpointQueueName,
                    UserMetadata = $"Events {endpointQueueName} subscribed to"
                };
                await managementClient.CreateSubscriptionAsync(subscriptionDescription);
            }
            
            //create audit queue
            if (!await managementClient.QueueExistsAsync(auditQueue))
                await managementClient.CreateQueueAsync(auditQueue);
            
            //create error queue
            if (!await managementClient.QueueExistsAsync(errorQueue))
                await managementClient.CreateQueueAsync(errorQueue);
        }
    }
}
