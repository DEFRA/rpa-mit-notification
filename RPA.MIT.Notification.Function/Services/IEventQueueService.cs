using System.Threading.Tasks;

namespace RPA.MIT.Notification.Function.Services;

public interface IEventQueueService
{
    Task CreateMessage(string id, string status, string action, string message, string data);
}