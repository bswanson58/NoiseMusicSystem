using Microsoft.Practices.Composite.Events;
using Noise.Infrastructure;
using NServiceBus;

namespace Noise.Service.Infrastructure.ServiceBus {
	public class LibraryUpdateStartedMessage : IMessage {
		public long		LibraryId { get; set; }
	}
  
	public class LibraryUpdateCompletedMessage : IMessage {
		public long		LibraryId { get; set; }
	}

	public class MessageHandlerBase {
		internal static IEventAggregator	EventAggregator;
	}

	public class LibraryUpdateStartedMessageHandler : MessageHandlerBase, IMessageHandler<LibraryUpdateStartedMessage> {
		public void Handle( LibraryUpdateStartedMessage message ) {
			if( EventAggregator != null ) {
				EventAggregator.GetEvent<Events.LibraryUpdateStarted>().Publish( message.LibraryId );
			}
		}
	}

	public class LibraryUpdateCompletedMessageHandler : MessageHandlerBase, IMessageHandler<LibraryUpdateCompletedMessage> {
		public void Handle( LibraryUpdateCompletedMessage message ) {
			if( EventAggregator != null ) {
				EventAggregator.GetEvent<Events.LibraryUpdateCompleted>().Publish( message.LibraryId );
			}
		}
	}
}
