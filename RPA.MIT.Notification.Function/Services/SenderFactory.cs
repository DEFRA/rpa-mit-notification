using Azure.Messaging.ServiceBus;

namespace RPA.MIT.Notification.Function.Services
{
	public class SenderFactory : ISenderFactory
	{
		public ServiceBusSender CreateSender(ServiceBusClient serviceBusClient, string queueName)
		{
            return serviceBusClient.CreateSender(queueName);
        }
	}
}

