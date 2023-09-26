using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
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

        var apiKey = configuration.GetSection("NotifyApiKey").Value;
//        Console.WriteLine("Startup apiKey=" + (string.IsNullOrEmpty(apiKey) ? "null" : apiKey.Substring(0, 40)));
        Console.WriteLine("Startup apiKey=" + apiKey);

        var queueConnectionString = configuration.GetSection("QueueConnectionString").Value;
        Console.WriteLine("Startup queueConnectionString=" + (string.IsNullOrEmpty(queueConnectionString) ? "null" : queueConnectionString.Substring(0, 90)));

        var eventQueueName = configuration.GetSection("EventQueueName").Value;
        Console.WriteLine("Startup eventQueueName=" + (string.IsNullOrEmpty(eventQueueName) ? "null" : eventQueueName));

        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<IEventQueueService>(_ =>
        {
            Console.WriteLine("queue client " + configuration.GetSection("QueueConnectionString").Value + " queueName=" + configuration.GetSection("EventQueueName").Value);
            var eventQueueClient = new QueueClient(configuration.GetSection("QueueConnectionString").Value, configuration.GetSection("EventQueueName").Value);
            return new EventQueueService(eventQueueClient);
        });
        services.AddSingleton<INotificationTable>(_ =>
        {
            Console.WriteLine("table client " + configuration.GetSection("TableConnectionString").Value + " table=" + configuration.GetSection("invoicenotification").Value);
            var tableClient = new TableClient(configuration.GetSection("TableConnectionString").Value, "invoicenotification");
            return new NotificationTable(tableClient);
        });
    })
    .Build();

host.Run();