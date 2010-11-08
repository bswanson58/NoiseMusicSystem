using NServiceBus;

namespace Noise.Service.ServiceBus {
	public class ServiceStatusMessage : IMessage {
		public string	Status { get; set; }

		public ServiceStatusMessage( string mesage ) {
			Status = mesage;
		}
	}

	public class ServiceStatusMessageHandler : IMessageHandler<ServiceStatusMessage> {
		public void Handle( ServiceStatusMessage message ) {
			var status = message.Status;
		}
	}
}
