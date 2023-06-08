using Azure;
using Azure.Data.Tables;
using RPA.MIT.Notification;
using RPA.MIT.Notification.Function.Models;
using RPA.MIT.Notification.Function.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using Xunit;
using System.Threading.Tasks;

namespace RPA.MIT.Notification.Function.Tests
{
    public class NotificationTests
    {
        private readonly Mock<INotificationTable> _mockNotificationTable;
        private readonly Mock<INotifyService> _mockNotifyService;
        private readonly Mock<IEventQueueService> _mockEventQueueService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<TableClient> _mockTableClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Notification _sut;

        public NotificationTests()
        {
            _mockNotificationTable = new Mock<INotificationTable>();
            _mockNotifyService = new Mock<INotifyService>();
            _mockEventQueueService = new Mock<IEventQueueService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();
            _mockTableClient = new Mock<TableClient>();

            _mockConfiguration.Setup(x => x["schemasAP"]).Returns("john.smith@defra.gov.uk");
            _mockConfiguration.Setup(x => x["templatesApproved"]).Returns("00000000-0000-0000-0000-000000000000");
            _sut = new  Notification(_mockNotificationTable.Object, _mockNotifyService.Object, _mockConfiguration.Object, _mockEventQueueService.Object, _mockLogger);
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
            _sut.CreateEvent(message);

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
            _sut.CreateEvent(message);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateEvent_Exception_Returns_Null_NotificationEntity()
        {
            Notification notifyFunction = new(null, null, null, null, null);
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
            await Assert.ThrowsAsync<NullReferenceException>(() => notifyFunction.CreateEvent(message));
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

            _sut.CreateEvent(message);

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

            _sut.CreateEvent(message);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }
    }
}