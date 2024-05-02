//using System;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
//using Moq;
//using RPA.MIT.Notification.Function.Services;
//using Xunit;

//namespace RPA.MIT.Notification.Function.Tests;

//public class EventQueueServiceTests
//{
//    [Fact]
//    public async Task CreateMessage_ValidArguments_CallsSendMessageAsync()
//    {
//        // Create IConfiguration
//        var configurationMock = new Mock<IConfiguration>();
//        var configurationSectionMock = new Mock<IConfigurationSection>();
//        configurationSectionMock.Setup(x => x.Value).Returns("ServiceBusConnectionString");
//        configurationMock.Setup(x => x.GetSection("ServiceBus:ConnectionString")).Returns(configurationSectionMock.Object);
//        configurationSectionMock.Setup(x => x.Value).Returns("YourQueueName");
//        configurationMock.Setup(x => x.GetSection("ServiceBus:QueueName")).Returns(configurationSectionMock.Object);

//        var serviceBusProviderMock = new Mock<ServiceBusProvider>();
        
//        // Create an instance of EventQueueService with the mock objects as dependencies
//        var eventQueueService = new EventQueueService(serviceBusProviderMock.Object, configurationMock.Object);

//        var id = Guid.NewGuid().ToString();
//        const string status = "new";
//        const string action = "create";
//        const string message = "Invoice created successfully";
//        const string data = "{\"Action\":\"approval\",\"Data\":{\"name\":\"SteveDickson\",\"link\":\"https://google.com\",\"schemeType\":\"bps\",\"value\":\"250\",\"invoiceId\":\"123456789\"},\"Scheme\":\"bps\",\"Id\":\"123456789\"}";

//        await eventQueueService.CreateMessage(id, status, action, message, data);

//        serviceBusProviderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
//    }
//}
