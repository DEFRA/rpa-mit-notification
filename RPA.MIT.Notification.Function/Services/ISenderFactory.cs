using Azure.Messaging.ServiceBus;

namespace RPA.MIT.Notification.Function.Services
{
	public interface ISenderFactory
	{
		ServiceBusSender CreateSender(ServiceBusClient serviceBusClient, string queueName);
	}
}

