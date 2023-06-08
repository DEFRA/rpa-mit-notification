using Azure;
using Azure.Data.Tables;
using RPA.MIT.Notification.Function.Models;
using RPA.MIT.Notification.Function.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using Xunit;

namespace RPA.MIT.Notification.Function.Tests
{
    public class NotificationTests
    {
        private readonly Mock<INotifyService> _mockNotifyService;
        private readonly Mock<IEventQueueService> _mockEventQueueService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<TableClient> _mockTableClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Notification _sut;

        public NotificationTests()
        {
            _mockNotifyService = new Mock<INotifyService>();
            _mockEventQueueService = new Mock<IEventQueueService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();
            _mockTableClient = new Mock<TableClient>();

            _mockConfiguration.Setup(x => x["schemasAP"]).Returns("john.smith@defra.gov.uk");
            _mockConfiguration.Setup(x => x["templatesApproved"]).Returns("00000000-0000-0000-0000-000000000000");
            _sut = new Notification(_mockNotifyService.Object, _mockConfiguration.Object, _mockEventQueueService.Object);
        }

        [Fact]
        public void CreateEvent_UnknownAction_Returns_Null_NotificationEntity()
        {
            var notificationRequest = new NotificationRequest
            {
                Action = "Unknown",
                InvoiceId = "123456",
                Scheme = "AP",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            string message = JsonConvert.SerializeObject(notificationRequest);
            _sut.CreateEvent(message, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }

        [Fact]
        public void CreateEvent_InvalidScheme_Returns_Null_NotificationEntity()
        {
            var notificationRequest = new NotificationRequest
            {
                Action = "Approved",
                InvoiceId = "123456",
                Scheme = "Other",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            string message = JsonConvert.SerializeObject(notificationRequest);
            _sut.CreateEvent(message, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateEvent_Exception_Returns_Null_NotificationEntity()
        {
            Notification notifyFunction = new(null, null, null);
            NotificationEntity? notificationEntity = null;
            var notificationRequest = new NotificationRequest
            {
                Action = "Approved",
                InvoiceId = "123456",
                Scheme = "AP",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            string message = JsonConvert.SerializeObject(notificationRequest);
            await Assert.ThrowsAsync<NullReferenceException>(() => notifyFunction.CreateEvent(message, _mockTableClient.Object, _mockLogger.Object));
            Assert.Null(notificationEntity);
        }

        [Fact]
        public void Given_Function_Receive_ValidMessage_Sends_Notification_Successfully()
        {
            EmailNotificationResponse emailResponse = new()
            {
                id = Guid.NewGuid().ToString(),
                reference = "Accounts Payable"
            };

            var notificationRequest = new NotificationRequest
            {
                Action = "Approved",
                InvoiceId = "123456",
                Scheme = "AP",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            string message = JsonConvert.SerializeObject(notificationRequest);
            _mockNotifyService.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JObject>())).Returns(emailResponse);

            _sut.CreateEvent(message, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Once);
        }

        [Fact]
        public void Given_Function_Receive_InValidMessage_Notification_ThrowsError()
        {
            EmailNotificationResponse emailResponse = new()
            {
                id = Guid.NewGuid().ToString(),
                reference = "Accounts Payable",
            };
            var inValidRequest = new NotificationRequest
            {
                Action = "Approved",
                Scheme = "AP"
            };

            string message = JsonConvert.SerializeObject(inValidRequest);

            _mockNotifyService.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(emailResponse);

            _sut.CreateEvent(message, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }

        [Fact]
        public void Given_CheckEmaliStatus_Returns_Null_When_Query_Table()
        {
            TimerSchedule schedule = new DailySchedule("2:00:00");
            TimerInfo timerInfo = new(schedule, It.IsAny<ScheduleStatus>(), false);

            _mockTableClient.Setup(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default));

            _sut.CheckEmailStatus(timerInfo, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default), Times.Once);
        }

        [Fact]
        public void Given_CheckEmaliStatus_Returns_Results_When_Query_Table()
        {
            TimerSchedule schedule = new DailySchedule("2:00:00");
            TimerInfo timerInfo = new(schedule, It.IsAny<ScheduleStatus>(), false);

            var notification = new Notify.Models.Notification { status = "delivered", id = "12355886" };

            var entity = new NotificationEntity()
            {
                PartitionKey = "12355886",
                RowKey = "12355886",
                Status = "sent",
                NotifyId = "12355886",
                Data = "test",
                RetryCount = 0,
                Timestamp = DateTime.UtcNow,
                ETag = ETag.All
            };
            var pagedValues = new[] { entity };

            var page = Page<NotificationEntity>.FromValues(pagedValues, default, new Mock<Response>().Object);
            var pageable = Pageable<NotificationEntity>.FromPages(new[] { page });

            _mockTableClient.Setup(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default)).Returns(pageable);
            _mockNotifyService.Setup(x => x.GetNotification(It.IsAny<string>())).Returns(notification);
            _mockTableClient.Setup(x => x.DeleteEntity(It.IsAny<string>(), It.IsAny<string>(), default, default));

            _sut.CheckEmailStatus(timerInfo, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default), Times.Once);
            _mockTableClient.Verify(x => x.DeleteEntity(It.IsAny<string>(), It.IsAny<string>(), default, default), Times.Once);
            _mockEventQueueService.Verify(x => x.CreateMessage(entity.PartitionKey, "delivered", "notification", "Email delivered", "test"), Times.Once);
        }

        [Theory]
        [InlineData("permanent-failure")]
        [InlineData("temporary-failure")]
        [InlineData("technical-failure")]
        public void Given_CheckEmaliStatus_Returns_Failure_When_Query_Table(string status)
        {
            TimerSchedule schedule = new DailySchedule("2:00:00");
            TimerInfo timerInfo = new(schedule, It.IsAny<ScheduleStatus>(), false);

            var notification = new Notify.Models.Notification { status = status, id = "12355886" };

            var entity = new NotificationEntity()
            {
                PartitionKey = "12355886",
                RowKey = "12355886",
                Status = "sent",
                NotifyId = "12355886",
                Data = "test",
                RetryCount = 0,
                Timestamp = DateTime.UtcNow,
                ETag = ETag.All
            };
            var pagedValues = new[] { entity };

            var mockResponse = new Mock<Response<TableEntity>>();
            mockResponse.Setup(x => x.Value).Returns(new TableEntity());

            var page = Page<NotificationEntity>.FromValues(pagedValues, default, new Mock<Response>().Object);
            var pageable = Pageable<NotificationEntity>.FromPages(new[] { page });

            _mockTableClient.Setup(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default)).Returns(pageable);
            _mockTableClient.Setup(x => x.GetEntity<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), default)).Returns(mockResponse.Object);
            _mockTableClient.Setup(x => x.UpdateEntity(It.IsAny<TableEntity>(), It.IsAny<ETag>(), TableUpdateMode.Replace, default));
            _mockNotifyService.Setup(x => x.GetNotification(It.IsAny<string>())).Returns(notification);

            _sut.CheckEmailStatus(timerInfo, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default), Times.Once);
            _mockTableClient.Verify(x => x.GetEntity<TableEntity>(entity.PartitionKey, entity.RowKey, null, default), Times.Once);
            _mockTableClient.Verify(x => x.UpdateEntity(It.IsAny<TableEntity>(), It.IsAny<ETag>(), TableUpdateMode.Replace, default), Times.Once);
            _mockEventQueueService.Verify(x => x.CreateMessage(entity.PartitionKey, status, "notification", $"Email {status}", "test"), Times.Once);
        }

        [Theory]
        [InlineData("unknown")]
        public void Given_CheckEmaliStatus_Returns_Failure_When_Query_Unknown(string status)
        {
            TimerSchedule schedule = new DailySchedule("2:00:00");
            TimerInfo timerInfo = new(schedule, It.IsAny<ScheduleStatus>(), false);

            var notification = new Notify.Models.Notification { status = status, id = "12355886" };

            var entity = new NotificationEntity()
            {
                PartitionKey = "12355886",
                RowKey = "12355886",
                Status = "sent",
                NotifyId = "12355886",
                Data = "test",
                RetryCount = 0,
                Timestamp = DateTime.UtcNow,
                ETag = ETag.All
            };
            var pagedValues = new[] { entity };

            var mockResponse = new Mock<Response<TableEntity>>();
            mockResponse.Setup(x => x.Value).Returns(new TableEntity());

            var page = Page<NotificationEntity>.FromValues(pagedValues, default, new Mock<Response>().Object);
            var pageable = Pageable<NotificationEntity>.FromPages(new[] { page });

            _mockTableClient.Setup(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default)).Returns(pageable);
            _mockTableClient.Setup(x => x.GetEntity<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), default)).Returns(mockResponse.Object);
            _mockTableClient.Setup(x => x.UpdateEntity(It.IsAny<TableEntity>(), It.IsAny<ETag>(), TableUpdateMode.Replace, default));
            _mockNotifyService.Setup(x => x.GetNotification(It.IsAny<string>())).Returns(notification);

            _sut.CheckEmailStatus(timerInfo, _mockTableClient.Object, _mockLogger.Object);

            _mockTableClient.Verify(x => x.Query<NotificationEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), default), Times.Once);
            _mockTableClient.Verify(x => x.GetEntity<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), default), Times.Never);
            _mockTableClient.Verify(x => x.UpdateEntity(It.IsAny<TableEntity>(), It.IsAny<ETag>(), TableUpdateMode.Replace, default), Times.Never);
            _mockEventQueueService.Verify(x => x.CreateMessage(entity.PartitionKey, "failed", "notification", "Email status not found", "test"), Times.Once);
        }
    }
}