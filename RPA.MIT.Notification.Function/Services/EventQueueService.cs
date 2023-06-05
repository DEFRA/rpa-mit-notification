using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace RPA.MIT.Notification.Function.Services;

public class EventQueueService : IEventQueueService
{
    private readonly QueueClient _queueClient;

    public EventQueueService(QueueClient queueClient)
    {
        _queueClient = queueClient;
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

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventRequest));
        await _queueClient.SendMessageAsync(Convert.ToBase64String(bytes));
    }
}