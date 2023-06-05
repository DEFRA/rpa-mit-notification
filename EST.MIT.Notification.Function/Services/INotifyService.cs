
using EST.MIT.Notification.Function.Models;
using Notify.Models.Responses;

namespace EST.MIT.Notification.Function.Services;

public interface INotifyService
{
    EmailNotificationResponse SendEmail(string email, string templateId, dynamic messagePersonalisation);
    Notify.Models.Notification GetNotification(string notificationId);
}
