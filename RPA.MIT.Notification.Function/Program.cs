using System;
using System.IO;
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
                    .AddJsonFile("local.settings.json")
                    .AddEnvironmentVariables())
    .ConfigureServices(services =>
    {
        Console.WriteLine("Startup.ConfigureServices() called");
        services.AddSingleton<INotificationClient>(sp =>
        {
            IConfiguration configuration = sp.GetService<IConfiguration>();
            return new NotificationClient(configuration["NotifyApiKey"]);
        });
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<IEventQueueService>(sp =>
        {
            IConfiguration configuration = sp.GetService<IConfiguration>();
            var eventQueueClient = new QueueClient(configuration["QueueConnectionString"], configuration["EventQueueName"]);
            return new EventQueueService(eventQueueClient);
        });
    })
    .Build();

host.Run();