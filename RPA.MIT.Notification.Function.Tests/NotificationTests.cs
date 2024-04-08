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
using Castle.Core.Logging;
using Azure.Messaging.ServiceBus;

namespace RPA.MIT.Notification.Function.Tests
{
    public class NotificationTests
    {
        private readonly Mock<NotificationTable> _mockNotificationTable;
        private readonly Mock<INotifyService> _mockNotifyService;
        private readonly Mock<IEventQueueService> _mockEventQueueService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<TableClient> _mockTableClient;
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;
        private readonly Notification _sut;

        public NotificationTests()
        {
            _mockNotifyService = new Mock<INotifyService>();
            _mockEventQueueService = new Mock<IEventQueueService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _loggerFactory = LoggerFactory.Create(c => c
               .AddConsole()
               .SetMinimumLevel(LogLevel.Debug)
               );
            _mockTableClient = new Mock<TableClient>();
            _mockNotificationTable = new Mock<NotificationTable>(_mockTableClient.Object);

            _mockConfiguration.Setup(x => x["schemasAP"]).Returns("john.smith@defra.gov.uk");
            _mockConfiguration.Setup(x => x["templatesApproved"]).Returns("00000000-0000-0000-0000-000000000000");
            _sut = new  Notification(_mockNotificationTable.Object, _mockNotifyService.Object, _mockConfiguration.Object, _mockEventQueueService.Object, _loggerFactory);
        }

        [Fact]
        public void SendNotification_UnknownAction_Returns_Null_NotificationEntity()
        {
            var notificationRequest = new NotificationRequest
            {
                Action = "Unknown",
                InvoiceId = "123456",
                Scheme = "AP",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            // Create a ServiceBusReceivedMessage and set its Body property
            var messageBody = new BinaryData(JsonConvert.SerializeObject(notificationRequest));
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

            // Pass the ServiceBusReceivedMessage to the function
            _sut.SendNotification(serviceBusReceivedMessage);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }

        [Fact]
        public void SendNotification_InvalidScheme_Returns_Null_NotificationEntity()
        {
            var notificationRequest = new NotificationRequest
            {
                Action = "Approved",
                InvoiceId = "123456",
                Scheme = "Other",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            // Create a ServiceBusReceivedMessage and set its Body property
            var messageBody = new BinaryData(JsonConvert.SerializeObject(notificationRequest));
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

            // Pass the ServiceBusReceivedMessage to the function
            _sut.SendNotification(serviceBusReceivedMessage);


            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendNotification_Exception_Returns_Null_NotificationEntity()
        {
            Notification notifyFunction = new(null, null, null, null, _loggerFactory);
            NotificationEntity? notificationEntity = null;
            var notificationRequest = new NotificationRequest
            {
                Action = "Approved",
                InvoiceId = "123456",
                Scheme = "AP",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            // Create a ServiceBusReceivedMessage and set its Body property
            var messageBody = new BinaryData(JsonConvert.SerializeObject(notificationRequest));
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

            await Assert.ThrowsAsync<NullReferenceException>(() => notifyFunction.SendNotification(serviceBusReceivedMessage));
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

            _mockNotifyService.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JObject>())).Returns(emailResponse);

            // Create a ServiceBusReceivedMessage and set its Body property
            var messageBody = new BinaryData(JsonConvert.SerializeObject(notificationRequest));
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

            // Pass the ServiceBusReceivedMessage to the function
            _sut.SendNotification(serviceBusReceivedMessage);

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

            _mockNotifyService.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(emailResponse);

            // Create a ServiceBusReceivedMessage and set its Body property
            var messageBody = new BinaryData(JsonConvert.SerializeObject(inValidRequest));
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);

            // Pass the ServiceBusReceivedMessage to the function
            _sut.SendNotification(serviceBusReceivedMessage);

            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<NotificationEntity>(), default), Times.Never);
        }
    }
}