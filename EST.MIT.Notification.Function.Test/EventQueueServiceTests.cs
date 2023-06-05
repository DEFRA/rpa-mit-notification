using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using EST.MIT.Notification.Function.Services;
using Moq;
using Xunit;

namespace EST.MIT.Notification.Function.Test;

public class EventQueueServiceTests
{
    [Fact]
    public async Task CreateMessage_ValidArguments_CallsSendMessageAsync()
    {
        var queueClientMock = new Mock<QueueClient>();
        var eventQueueService = new EventQueueService(queueClientMock.Object);
        var id = Guid.NewGuid().ToString();
        const string status = "new";
        const string action = "create";
        const string message = "Invoice created successfully";
        const string data = "{\"Action\":\"approval\",\"Data\":{\"name\":\"SteveDickinson\",\"link\":\"https://google.com\",\"schemeType\":\"bps\",\"value\":\"250\",\"invoiceId\":\"123456789\"},\"Scheme\":\"bps\",\"Id\":\"123456789\"}";

        await eventQueueService.CreateMessage(id, status, action, message, data);

        queueClientMock.Verify(qc => qc.SendMessageAsync(It.IsAny<string>()), Times.Once);
    }
}
