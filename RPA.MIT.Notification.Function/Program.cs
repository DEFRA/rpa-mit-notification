using System;
using System.IO;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
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
        Console.WriteLine("Startup.ConfigureServices()1 called");
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var qn1 = configuration.GetSection("ServiceBusEventQueueName").Value;
        Console.WriteLine("Startup ServiceBus eventQueueName1 = " + qn1);
        var qn2 = configuration.GetSection("ServiceBusNotificationQueueName").Value;
        Console.WriteLine("Startup ServiceBus notificationQueueName1 = " + qn2);
        var mi = configuration.GetSection("ServiceBusEventConnectionString:fullyQualifiedNamespace").Value;
        Console.WriteLine("Startup ServiceBus endpoint1 = " + mi);

        services.AddScoped<ISenderFactory, SenderFactory>();
        Console.WriteLine("Startup added SenderFactory");
        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        Console.WriteLine("Startup added NotificationClient");
        services.AddSingleton<INotifyService, NotifyService>();
        var apiKey = configuration.GetSection("NotifyApiKey").Value;
        Console.WriteLine("Startup apiKey = " + (string.IsNullOrEmpty(apiKey) ? "null" : apiKey.Substring(0, 50)));
        Console.WriteLine("Startup added NotificationService");
        /*services.AddSingleton<IEventQueueService>(_ =>
        {
            // Constructors are slightly different dpending if using Managed Identity or SAS connection string
            var managedIdentityNamespace = configuration.GetSection("ServiceBusEventConnectionString:fullyQualifiedNamespace").Value;
            Console.WriteLine("Startup ServiceBus endpoint2 = " + managedIdentityNamespace);
            var connectionString = configuration.GetSection("ServiceBusEventConnectionString").Value;
            var serviceBusClient = string.IsNullOrEmpty(managedIdentityNamespace)
                ? new ServiceBusClient(connectionString)
                : new ServiceBusClient(managedIdentityNamespace, new DefaultAzureCredential()); // ManagedIdentityCredential());
            var queueName = configuration.GetSection("ServiceBusEventQueueName").Value;
            Console.WriteLine("Startup ServiceBus queueName2 = " + queueName);
            return new EventQueueService(serviceBusClient, queueName, new SenderFactory());
        });
        Console.WriteLine("Startup added EventQueueService");
        services.AddSingleton<INotificationTable>(_ =>
        {
            var tableClient = new TableClient(configuration.GetSection("TableConnectionString").Value, "invoicenotification");
            return new NotificationTable(tableClient);
        });
        Console.WriteLine("Startup added NotificationTable"); 
        */
    })
    .Build();

Console.WriteLine("Startup Build() finished");
host.Run();
Console.WriteLine("Startup Run");
