using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notify.Client;
using Notify.Interfaces;
using RPA.MIT.Notification.Function.Services;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        if(hostContext.HostingEnvironment.IsDevelopment())
        {
            Console.WriteLine("STARTING IN DEVELOPMENT MODE");
            config.AddUserSecrets<Program>();
        }
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        Console.WriteLine("Startup.ConfigureServices() called");
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<IServiceBusProvider, ServiceBusProvider>();

        services.AddQueueAndTableServices(configuration);
    })
    .Build();

Console.WriteLine("Startup.ConfigureServices() completed");

host.Run();
