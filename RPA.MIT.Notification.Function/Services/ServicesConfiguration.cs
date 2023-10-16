using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using Azure.Storage.Queues;
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
                    var queueServiceUrl = configuration.GetSection("QueueConnectionString:QueueServiceUri").Value;
                    var queueUri = new Uri($"{queueServiceUrl}{queueName}");
                    Console.WriteLine($"Startup.QueueClient using Managed Identity with url {queueUri}");
                    return new EventQueueService(new QueueClient(queueUri, new DefaultAzureCredential()));
                }
                else
                {
                    return new EventQueueService(new QueueClient(configuration.GetSection("QueueConnectionString").Value, queueName));
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