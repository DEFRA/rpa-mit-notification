using System;
using System.IO;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notify.Client;
using Notify.Interfaces;
using RPA.MIT.Notification.Function.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(config => config
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables())
    .ConfigureServices(services =>
    {
        Console.WriteLine("Startup.ConfigureServices() called");
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<IEventQueueService>(_ =>
        {
            var eventQueueClient = new QueueClient(configuration.GetSection("QueueConnectionString").Value, configuration.GetSection("EventQueueName").Value);
            return new EventQueueService(eventQueueClient);
        });
        services.AddSingleton<INotificationTable>(_ =>
        {
            var tableClient = new TableClient(configuration.GetSection("TableConnectionString").Value, "invoicenotification");
            return new NotificationTable(tableClient);
        });
    })
    .Build();

host.Run();