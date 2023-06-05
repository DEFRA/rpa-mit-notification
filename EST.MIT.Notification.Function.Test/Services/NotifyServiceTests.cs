using EST.MIT.Notification.Function.Services;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notify.Interfaces;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using Xunit;

namespace EST.MIT.Notification.Function.Test.Services
{
    public class NotifyServiceTests
    {

        private readonly Mock<INotificationClient> _mockClient;
        private readonly INotifyService _notifyService;
        public NotifyServiceTests()
        {
            _mockClient = new Mock<INotificationClient>();
            _notifyService = new NotifyService(_mockClient.Object);
        }

        [Fact]
        public void Given_NotificationDetails_NotifyServiceSendsAnEmail_Successfully()
        {
            var notificationResponse = new EmailNotificationResponse { id = Guid.NewGuid().ToString(), reference = "123456" };
            _mockClient.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(notificationResponse);
            var notificationRequest = new NotificationRequest
            {
                Action = "Approved",
                InvoiceId = "123456",
                Scheme = "AP",
                Id = "001",
                Data = new Personalisation { TemplateField1 = "approved" }
            };

            string message = JsonConvert.SerializeObject(notificationRequest);
            dynamic notificationMsgObj = JObject.Parse(message);
            var notificationTypeObj = notificationMsgObj.Data;

            var response = _notifyService.SendEmail("qasim.javed@defra.gov.uk", "approved", notificationTypeObj);

            Assert.NotNull(response);
            Assert.Equal("123456", response.reference);
        }

        [Fact]
        public void Given_NotificationId_NotifyService_GetNotification_Returns_Notification()
        {
            var notificationResponse = new Notify.Models.Notification { id = Guid.NewGuid().ToString(), reference = "123456" };
            _mockClient.Setup(x => x.GetNotificationById(It.IsAny<string>())).Returns(notificationResponse);

            var response = _notifyService.GetNotification("123456");

            Assert.NotNull(response);
            Assert.Equal("123456", response.reference);
        }
    }
}
