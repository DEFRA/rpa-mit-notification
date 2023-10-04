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
        Console.WriteLine("Startup.ConfigureServices() called");
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        services.AddScoped<ISenderFactory, SenderFactory>();
        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<IEventQueueService>(_ =>
        {
            // Constructors are slightly different dpending if using Managed Identity or SAS connection string
            var managedIdentityNamespace = configuration.GetSection("ServiceBusEventConnectionString:fullyQualifiedNamespace").Value;
            var connectionString = configuration.GetSection("ServiceBusEventConnectionString").Value;
            var serviceBusClient = string.IsNullOrEmpty(managedIdentityNamespace)
                ? new ServiceBusClient(connectionString)
                : new ServiceBusClient(managedIdentityNamespace, new DefaultAzureCredential()); // ManagedIdentityCredential());
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