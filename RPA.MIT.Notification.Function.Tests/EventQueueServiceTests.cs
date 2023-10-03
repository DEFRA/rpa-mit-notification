using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using RPA.MIT.Notification.Function.Services;
using Moq;
using Xunit;
using Azure.Messaging.ServiceBus;

namespace RPA.MIT.Notification.Function.Tests;

public class EventQueueServiceTests
{
    [Fact]
    public async Task CreateMessage_ValidArguments_CallsSendMessageAsync()
    {
        var serviceBusClientMock = new Mock<ServiceBusClient>();
        var serviceBusSenderMock = new Mock<ServiceBusSender>();
        var senderFactoryMock = new Mock<ISenderFactory>();
        senderFactoryMock.Setup(x => x.CreateSender(It.IsAny<ServiceBusClient>(), It.IsAny<string>())).Returns(serviceBusSenderMock.Object);
        var eventQueueService = new EventQueueService(serviceBusClientMock.Object, "queueName", senderFactoryMock.Object);
        var id = Guid.NewGuid().ToString();
        const string status = "new";
        const string action = "create";
        const string message = "Invoice created successfully";
        const string data = "{\"Action\":\"approval\",\"Data\":{\"name\":\"SteveDickinson\",\"link\":\"https://google.com\",\"schemeType\":\"bps\",\"value\":\"250\",\"invoiceId\":\"123456789\"},\"Scheme\":\"bps\",\"Id\":\"123456789\"}";

        await eventQueueService.CreateMessage(id, status, action, message, data);

        serviceBusSenderMock.Verify(qc => qc.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
    }
}
