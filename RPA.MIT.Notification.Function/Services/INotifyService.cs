
using RPA.MIT.Notification.Function.Models;
using Notify.Models.Responses;

namespace RPA.MIT.Notification.Function.Services;

public interface INotifyService
{
    EmailNotificationResponse SendEmail(string email, string templateId, dynamic messagePersonalisation);
    Notify.Models.Notification GetNotification(string notificationId);
}
