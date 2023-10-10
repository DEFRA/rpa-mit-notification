using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notify.Client;
using Notify.Interfaces;
using RPA.MIT.Notification.Function.Services;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config => config
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables())
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        Console.WriteLine("Startup.ConfigureServices() called");
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        services.AddSingleton<INotifyService, NotifyService>();

        var storageAccountCredential = configuration.GetSection("StorageAccount:Credential").Value;
        if (IsManagedIdentity(storageAccountCredential))
        {
            Console.WriteLine("Startup.QueueClient/TableClient using Managed Identity");
        }
        else
        {
            Console.WriteLine("Startup.QueueClient/TableClient using Connection String");
        }

        services.AddSingleton<IEventQueueService>(_ =>
        {
            var queueName = configuration.GetSection("EventQueueName").Value;
            if (IsManagedIdentity(storageAccountCredential))
            {
                var queueServiceUri = configuration.GetSection("StorageAccount:QueueServiceUri").Value;
                var queueUrl = new Uri($"{queueServiceUri}{queueName}");
                return new EventQueueService(new QueueClient(queueUrl, new DefaultAzureCredential()));
            }
            else
            {
                return new EventQueueService(new QueueClient(configuration.GetSection("QueueConnectionString").Value, queueName));
            }
        });
        services.AddSingleton<INotificationTable>(_ =>
        {
            var tableName = configuration.GetSection("NotificationTableName").Value;
            if (IsManagedIdentity(storageAccountCredential))
            {
                var tableServiceUri = new Uri(configuration.GetSection("TableServiceUri").Value);
                return new NotificationTable(new TableClient(tableServiceUri, tableName, new DefaultAzureCredential()));
            }
            else
            {
                return new NotificationTable(new TableClient(configuration.GetSection("TableConnectionString").Value, tableName));
            }
        });
    })
    .Build();

host.Run();

static bool IsManagedIdentity(string credentialName)
{
    return (credentialName != null && credentialName.ToLower() == "managedidentity");
}