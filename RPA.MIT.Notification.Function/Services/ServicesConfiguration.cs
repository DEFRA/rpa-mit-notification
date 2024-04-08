using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Azure.Data.Tables;

namespace RPA.MIT.Notification.Function.Services
{
    /// <summary>
    /// Register service-tier services.
    /// </summary>
    public static class ServicesConfiguration
    {
        /// <summary>
        /// Method to register service-tier services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddQueueAndTableServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IEventQueueService>(_ =>
            {
                var queueCredential = configuration.GetSection("QueueConnectionString:Credential").Value;
                var queueName = configuration.GetSection("EventQueueName").Value;
                if (IsManagedIdentity(queueCredential))
                {
                    var serviceBusNamespace = configuration.GetSection("QueueConnectionString:ServiceBusNamespace").Value;
                    var fullyQualifiedNamespace = $"{serviceBusNamespace}.servicebus.windows.net";
                    Console.WriteLine($"Startup.ServiceBusClient using Managed Identity with namespace {fullyQualifiedNamespace}");
                    
                    // Using Azure.Messaging.ServiceBus with Managed Identity
                    var client = new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential());
                    return new EventQueueService(new ServiceBusProvider(configuration), configuration);
                }
                else
                {
                    // Using Azure.Messaging.ServiceBus with a connection string
                    var connectionString = configuration.GetSection("QueueConnectionString").Value;
                    var client = new ServiceBusClient(connectionString);
                    return new EventQueueService(new ServiceBusProvider(configuration), configuration);
                }
            });

            services.AddSingleton<INotificationTable>(_ =>
            {
                var tableCredential = configuration.GetSection("TableConnectionString:Credential").Value;
                var tableName = configuration.GetSection("NotificationTableName").Value;
                if (IsManagedIdentity(tableCredential))
                {
                    var tableServiceUri = new Uri(configuration.GetSection("TableConnectionString:TableServiceUri").Value);
                    Console.WriteLine($"Startup.TableClient using Managed Identity with url {tableServiceUri}");
                    return new NotificationTable(new TableClient(tableServiceUri, tableName, new DefaultAzureCredential()));
                }
                else
                {
                    return new NotificationTable(new TableClient(configuration.GetSection("TableConnectionString").Value, tableName));
                }
            });
        }

        private static bool IsManagedIdentity(string credentialName)
        {
            return credentialName != null && credentialName.ToLower() == "managedidentity";
        }
    }
}