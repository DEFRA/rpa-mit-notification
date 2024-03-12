using System;
using System.Threading.Tasks;
using RPA.MIT.Notification.Function.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Services.ServiceBusProvider;

namespace RPA.MIT.Notification.Function.Tests;

public class EventQueueServiceTests
{
    [Fact]
    public async Task CreateMessage_ValidArguments_CallsSendMessageAsync()
    {
        // Create a mock object for IConfiguration
        var configurationMock = new Mock<IConfiguration>();
        // Create a mock object for IConfigurationSection
        var configurationSectionMock = new Mock<IConfigurationSection>();
        
        // Setup the mock object to return a specific value when the Value property is accessed
        configurationSectionMock.Setup(x => x.Value).Returns("ServiceBusConnectionString");
        // Setup the mock object to return the mock IConfigurationSection when GetSection is called with "ServiceBus:ConnectionString"
        configurationMock.Setup(x => x.GetSection("ServiceBus:ConnectionString")).Returns(configurationSectionMock.Object);
        
        // Setup the mock object to return a specific value when the Value property is accessed
        configurationSectionMock.Setup(x => x.Value).Returns("YourQueueName");
        // Setup the mock object to return the mock IConfigurationSection when GetSection is called with "ServiceBus:QueueName"
        configurationMock.Setup(x => x.GetSection("ServiceBus:QueueName")).Returns(configurationSectionMock.Object);

        // Create a mock object for ServiceBusProvider
        var serviceBusProviderMock = new Mock<ServiceBusProvider>();
        
        // Create an instance of EventQueueService with the mock objects as dependencies
        var eventQueueService = new EventQueueService(serviceBusProviderMock.Object, configurationMock.Object);
                
        // Define test data
        var id = Guid.NewGuid().ToString();
        const string status = "new";
        const string action = "create";
        const string message = "Invoice created successfully";
        const string data = "{\"Action\":\"approval\",\"Data\":{\"name\":\"SteveDickson\",\"link\":\"https://google.com\",\"schemeType\":\"bps\",\"value\":\"250\",\"invoiceId\":\"123456789\"},\"Scheme\":\"bps\",\"Id\":\"123456789\"}";

        // Call the method under test
        await eventQueueService.CreateMessage(id, status, action, message, data);

        // Verify that the SendMessageAsync method was called exactly once with any string values as parameters
        serviceBusProviderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
