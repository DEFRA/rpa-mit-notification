using Azure;
using Azure.Data.Tables;
using EST.MIT.Notification.Function.Models;
using EST.MIT.Notification.Function.Services;
using EST.MIT.Notification.Function.Validation;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace EST.MIT.Notification
{
    public class Notification
    {
        private readonly INotifyService _notifyService;
        private readonly IConfiguration _configuration;

        private readonly IEventQueueService _eventQueueService;

        public Notification(INotifyService notifyService, IConfiguration configuration, IEventQueueService eventQueueService)
        {
            _notifyService = notifyService;
            _configuration = configuration;
            _eventQueueService = eventQueueService;
        }

        [FunctionName("SendNotification")]
        public async Task CreateEvent(
            [QueueTrigger("invoicenotification", Connection = "QueueConnectionString")] string notificationMsg,
            [Table("invoicenotification", Connection = "TableConnectionString")] TableClient tableClient,
            ILogger log)
        {
            log.LogInformation("MIT Notification queue trigger function processing: {notificationMsg}", notificationMsg);

            try
            {
                var isValid = ValidateMessage.IsValid(notificationMsg);

                if (!isValid)
                {
                    log.LogError("Invalid message: {notificationMsg}", notificationMsg);
                    return;
                }

                dynamic notificationMsgObj = JObject.Parse(notificationMsg);
                string templateName = notificationMsgObj.Action;
                string scheme = notificationMsgObj.Scheme;
                var templateId = _configuration[$"templates:{templateName}"];
                var emailAddress = _configuration[$"schemas:{scheme}"];
                string id = notificationMsgObj.Id;

                if (templateId == null)
                {
                    log.LogError("Template not found for action: {action}", templateName);
                    await _eventQueueService.CreateMessage(id, "failed", "notification", "Template not found", notificationMsg);
                    return;
                }

                if (emailAddress == null)
                {
                    log.LogError("emailAddress not found for scheme: {scheme}", scheme);
                    await _eventQueueService.CreateMessage(id, "failed", "notification", "emailAddress not found", notificationMsg);
                    return;
                }

                log.LogInformation("Sending email for incoming message id: {id}", id);

                var notifyResponse = _notifyService.SendEmail(emailAddress, templateId, notificationMsgObj.Data);
                await _eventQueueService.CreateMessage(id, "sent", "notification", "Email sent", notificationMsg);
                var notificationEntity = new NotificationEntity()
                {
                    PartitionKey = id,
                    RowKey = Guid.NewGuid().ToString(),
                    Status = "sent",
                    NotifyId = notifyResponse.id,
                    RetryCount = 0,
                    Data = notificationMsg
                };
                await tableClient.AddEntityAsync(notificationEntity);
            }
            catch (Exception exc)
            {
                log.LogError(exc, "Error occurred processing incoming message");
                throw;  // This will force a retry to prevent losing the message.
            }
        }

        [FunctionName("CheckEmailStatus")]
        public async Task CheckEmailStatus(
            [TimerTrigger("%TriggerTimerInterval%", RunOnStartup = true)] TimerInfo myTimer,
            [Table("invoicenotification", Connection = "TableConnectionString")] TableClient tableClient,
            ILogger log)
        {
            log.LogInformation("CheckEmailStatus function executed at: {time}", DateTime.Now);

            var queryResultsFilter = tableClient.Query<NotificationEntity>(filter: "Status eq 'sent'");

            if (queryResultsFilter != null)
            {
                foreach (var result in queryResultsFilter)
                {
                    log.LogInformation($"Checking email status for {result.NotifyId}");
                    var emailStatus = _notifyService.GetNotification(result.NotifyId);
                    log.LogInformation($"Email status for {result.NotifyId} = {emailStatus.status}");

                    TableEntity entity;
                    switch (emailStatus.status)
                    {
                        case "delivered":
                            log.LogInformation($"Email status for {result.NotifyId} is 'delivered' entity is removed from table.");
                            tableClient.DeleteEntity(result.PartitionKey, result.RowKey);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "delivered", "notification", "Email delivered", result.Data);
                            break;

                        case "permanent-failure":
                            log.LogWarning($"Email status for {result.NotifyId} is 'permanent-failure'");
                            entity = tableClient.GetEntity<TableEntity>(result.PartitionKey, result.RowKey);
                            entity["Status"] = "Permanent Failure";
                            tableClient.UpdateEntity(entity, ETag.All, TableUpdateMode.Replace);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "permanent-failure", "notification", "Email permanent-failure", result.Data);
                            break;

                        case "temporary-failure":
                            log.LogWarning($"Email status for {result.NotifyId} is 'temporary-failure'");
                            entity = tableClient.GetEntity<TableEntity>(result.PartitionKey, result.RowKey);
                            entity["Status"] = "Temporary Failure";
                            tableClient.UpdateEntity(entity, ETag.All, TableUpdateMode.Replace);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "temporary-failure", "notification", "Email temporary-failure", result.Data);
                            break;

                        case "technical-failure":
                            log.LogWarning($"Email status for {result.NotifyId} is 'technical-failure'");
                            entity = tableClient.GetEntity<TableEntity>(result.PartitionKey, result.RowKey);
                            entity["Status"] = "Technical Failure";
                            tableClient.UpdateEntity(entity, ETag.All, TableUpdateMode.Replace);
                            await _eventQueueService.CreateMessage(result.PartitionKey, "technical-failure", "notification", "Email technical-failure", result.Data);
                            break;

                        default:
                            log.LogWarning($"Email status for {result.NotifyId} not found");
                            await _eventQueueService.CreateMessage(result.PartitionKey, "failed", "notification", "Email status not found", result.Data);
                            break;
                    }
                }
            }
        }
    }
}
