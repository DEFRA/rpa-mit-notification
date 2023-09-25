using Azure;
using Azure.Data.Tables;
using RPA.MIT.Notification.Function.Models;
using RPA.MIT.Notification.Function.Services;
using RPA.MIT.Notification.Function.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using System.Collections.Generic;

namespace RPA.MIT.Notification
{
    public class Notification
    {
        private readonly INotificationTable _notificationTable;
        private readonly INotifyService _notifyService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IEventQueueService _eventQueueService;

        public Notification(INotificationTable notificationTable,  INotifyService notifyService, IConfiguration configuration, IEventQueueService eventQueueService, ILoggerFactory loggerFactory)
        {
            _notificationTable = notificationTable;
            _notifyService = notifyService;
            _configuration = configuration;
            _eventQueueService = eventQueueService;
            _logger = loggerFactory.CreateLogger<Notification>();
        }

        [Function("SendNotification")]
        public async Task CreateEvent(
            [QueueTrigger("invoicenotification", Connection = "QueueConnectionString")] string notificationMsg)
        {
            _logger.LogInformation("MIT Notification queue trigger function processing: {notificationMsg}", notificationMsg);

            try
            {
                var isValid = ValidateMessage.IsValid(notificationMsg);

                if (!isValid)
                {
                    _logger.LogError("Invalid message: {notificationMsg}", notificationMsg);
                }

                dynamic notificationMsgObj = JObject.Parse(notificationMsg);
                string templateName = notificationMsgObj.Action;
                string scheme = notificationMsgObj.Scheme;
                var templateId = _configuration[$"templates{templateName}"];
                var emailAddress = _configuration[$"schemas{scheme}"];
                string id = notificationMsgObj.Id;

                if (templateId == null)
                {
                    _logger.LogError("Template not found for action: {action}", templateName);
                    await _eventQueueService.CreateMessage(id, "failed", "notification", "Template not found", notificationMsg);
                }

                if (emailAddress == null)
                {
                    _logger.LogError("emailAddress not found for scheme: {scheme}", scheme);
                    await _eventQueueService.CreateMessage(id, "failed", "notification", "emailAddress not found", notificationMsg);
                }

                _logger.LogInformation("Sending email for incoming message id: {id}", id);

                var notifyResponse = _notifyService.SendEmail(emailAddress, templateId, notificationMsgObj.Data);
                await _eventQueueService.CreateMessage(id, "sent", "notification", "Email sent", notificationMsg);
                await _notificationTable.Add(new NotificationEntity()
                {
                    PartitionKey = id,
                    RowKey = Guid.NewGuid().ToString(),
                    Status = "sent",
                    NotifyId = notifyResponse.id,
                    RetryCount = 0,
                    Data = notificationMsg
                });
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error occurred processing incoming message");
                throw;  // This will force a retry to prevent losing the message.
            }
        }

        [Function("CheckConfig")]
        public void CheckConfig(
            [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
        {
            Console.WriteLine("CheckConfig function executed - console");
            _logger.LogInformation("CheckConfig function executed at: {time}", DateTime.Now);
            var queueConnString = _configuration.GetSection("QueueConnectionString").Value;
            _logger.LogInformation("CheckConfig queueConnectionString=" + (string.IsNullOrEmpty(queueConnString) ? "null" : queueConnString.Substring(0,40)));
            Console.WriteLine("CheckConfig queueConnectionString - console=" + (string.IsNullOrEmpty(queueConnString) ? "null" : queueConnString.Substring(0,40)));
        }

        [Function("CheckConfigStatic")]
        public static void CheckConfigStatic(
            [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
        {
            Console.WriteLine("CheckConfigStatic function executed - console");
        }

        /*
        [Function("CheckEmailStatus")]
        public async Task CheckEmailStatus(
            [TimerTrigger("%TriggerTimerInterval%")] TimerInfo myTimer)
        {
            _logger.LogInformation("CheckEmailStatus function executed at: {time}", DateTime.Now);

            var queryResultsFilter = _notificationTable.RetrieveActive();

            if (queryResultsFilter != null)
            {
                foreach (var result in queryResultsFilter)
                {
                    _logger.LogInformation($"Checking email status for {result.NotifyId}");
                    var emailStatus = _notifyService.GetNotification(result.NotifyId);
                    _logger.LogInformation($"Email status for {result.NotifyId} = {emailStatus.status}");

                    TableEntity entity;
                    switch (emailStatus.status)
                    {
                        case "delivered":
                            _logger.LogInformation($"Email status for {result.NotifyId} is 'delivered' entity is removed from table.");
                            await _notificationTable.Delete(result.PartitionKey, result.RowKey);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "delivered", "notification", "Email delivered", result.Data);
                            break;

                        case "permanent-failure":
                            _logger.LogWarning($"Email status for {result.NotifyId} is 'permanent-failure'");
                            entity = await _notificationTable.Get(result.PartitionKey, result.RowKey);
                            entity["Status"] = "Permanent Failure";
                            await _notificationTable.Update(entity);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "permanent-failure", "notification", "Email permanent-failure", result.Data);
                            break;

                        case "temporary-failure":
                            _logger.LogWarning($"Email status for {result.NotifyId} is 'temporary-failure'");
                            entity = await _notificationTable.Get(result.PartitionKey, result.RowKey);
                            entity["Status"] = "Temporary Failure";
                            await _notificationTable.Update(entity);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "temporary-failure", "notification", "Email temporary-failure", result.Data);
                            break;

                        case "technical-failure":
                            _logger.LogWarning($"Email status for {result.NotifyId} is 'technical-failure'");
                            entity = await _notificationTable.Get(result.PartitionKey, result.RowKey);
                            entity["Status"] = "Technical Failure";
                            await _notificationTable.Update(entity);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "technical-failure", "notification", "Email technical-failure", result.Data);
                            break;

                        default:
                            _logger.LogWarning($"Email status for {result.NotifyId} not found");
                            await _eventQueueService.CreateMessage(result.PartitionKey, "failed", "notification", "Email status not found", result.Data);
                            break;
                    }
                }
            }
        }
        */
    }
}
