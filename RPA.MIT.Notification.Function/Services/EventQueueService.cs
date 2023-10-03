using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace RPA.MIT.Notification.Function.Services;

public class EventQueueService : IEventQueueService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName;
    private readonly ISenderFactory _senderFactory;

    public EventQueueService(ServiceBusClient serviceBusClient, string queueName, ISenderFactory senderFactory)
    {
        _serviceBusClient = serviceBusClient;
        _queueName = queueName;
        _senderFactory = senderFactory;
    }

    public async Task CreateMessage(string id, string status, string action, string message, string data)
    {
        var eventRequest = new Event()
        {
            Name = "Notification",
            Properties = new EventProperties()
            {
                Id = id,
                Status = status,
                Checkpoint = "Notification Api",
                Action = new EventAction()
                {
                    Type = action,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Data = data
                }
            }
        };

        var eventStr = JsonSerializer.Serialize(eventRequest);
        var serviceBusSender = _senderFactory.CreateSender(_serviceBusClient, _queueName);
        await serviceBusSender.SendMessageAsync(new ServiceBusMessage(eventStr));
    }
}