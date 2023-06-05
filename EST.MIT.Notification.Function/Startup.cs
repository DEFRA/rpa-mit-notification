using Azure.Storage.Queues;
using EST.MIT.Notification.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Notify.Client;
using Notify.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: FunctionsStartup(typeof(EST.MIT.Notification.Startup))]
namespace EST.MIT.Notification
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Console.WriteLine("Startup.Configure() called");
            var notifyApi = builder.GetContext().Configuration["NotifyApiKey"];
            var eventQueueName = builder.GetContext().Configuration["EventQueueName"];
            var queueConnectionString = builder.GetContext().Configuration["QueueConnectionString"];
            builder.Services.AddSingleton<INotificationClient>((_) => new NotificationClient(notifyApi));
            builder.Services.AddSingleton<INotifyService, NotifyService>();
            builder.Services.AddSingleton<IEventQueueService>(_ =>
            {
                var eventQueueClient = new QueueClient(queueConnectionString, eventQueueName);
                return new EventQueueService(eventQueueClient);
            });
        }
    }
}
