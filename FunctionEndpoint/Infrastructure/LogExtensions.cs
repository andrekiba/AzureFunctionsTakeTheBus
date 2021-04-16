using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace Elfo.NsbFunctions.FunctionEndpoint.Infrastructure
{
    public static class LogExtensions
    {
        public static void ConfigureLogger(this IServiceCollection services, IConfiguration configuration)
        {
            var columnOpts = new ColumnOptions();
            columnOpts.Store.Remove(StandardColumn.Properties);
            columnOpts.Store.Remove(StandardColumn.MessageTemplate);
            columnOpts.Store.Add(StandardColumn.LogEvent);
            columnOpts.AdditionalColumns = new List<SqlColumn>
            {
                new("Application", SqlDbType.VarChar, dataLength: 50),
                new("SourceContext", SqlDbType.NVarChar),
                new("UserName", SqlDbType.NVarChar, dataLength: 50),
                new("OperationId", SqlDbType.UniqueIdentifier)
            };

            var logger = new LoggerConfiguration()
                /*
                .WriteTo.MSSqlServer(configuration.GetConnectionString("Db"),
                    tableName: "logs_tb",
                    schemaName: "log",
                    autoCreateSqlTable: true,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    columnOptions: columnOpts)
                */
                .ReadFrom.Configuration(configuration, sectionName: "AzureFunctionsJobHost:Serilog", DependencyContext.Load(typeof(Startup).Assembly))
                .Enrich.WithProperty("Application", configuration["AppName"])
                .CreateLogger();
            
            services.AddLogging(lb => lb.AddSerilog(logger));
        }
    }
}