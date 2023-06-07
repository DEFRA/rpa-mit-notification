using System;
using System.Collections.Generic;
using System.Linq;
using RPA.MIT.Notification.Function.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Notify.Client;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace RPA.MIT.Notification.Function.Services;

public class NotifyService : INotifyService
{
    private readonly INotificationClient _notifyServiceClient;

    public NotifyService(INotificationClient notifyServiceClient)
    {
        _notifyServiceClient = notifyServiceClient;
    }

    public Notify.Models.Notification GetNotification(string notificationId)
    {
        return _notifyServiceClient.GetNotificationById(notificationId);
    }

    public EmailNotificationResponse SendEmail(string email, string templateId, dynamic messagePersonalisation)
    {
        Dictionary<string, dynamic> personalisation = new();

        foreach (var j in messagePersonalisation)
        {
            var jp = (JProperty)j;
            var j1 = jp[0];

            personalisation.Add(jp.Name, j1.ToString());
        }

        return _notifyServiceClient.SendEmail(
                    email,
                    templateId,
                    personalisation
                );
    }
}
