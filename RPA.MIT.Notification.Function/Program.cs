using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Messaging.ServiceBus;
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

        services.AddScoped<ISenderFactory, SenderFactory>();
        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<IEventQueueService>(_ =>
        {
            var serviceBusClient = new ServiceBusClient(configuration.GetSection("ServiceBusEventConnectionString").Value);
            var queueName = configuration.GetSection("ServiceBusEventQueueName").Value;
            return new EventQueueService(serviceBusClient, queueName, new SenderFactory());
        });
        services.AddSingleton<INotificationTable>(_ =>
        {
            var tableClient = new TableClient(configuration.GetSection("TableConnectionString").Value, "invoicenotification");
            return new NotificationTable(tableClient);
        });
    })
    .Build();

host.Run();