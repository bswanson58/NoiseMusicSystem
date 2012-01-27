using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using NServiceBus;

namespace Noise.Service.Infrastructure.ServiceBus {
	public class MessageHandlerBase {
		internal static ICaliburnEventAggregator	EventAggregator;
	}

	public class LibraryUpdateStartedMessage : IMessage {
		public long		LibraryId { get; set; }
	}
  
	public class LibraryUpdateStartedMessageHandler : MessageHandlerBase, IMessageHandler<LibraryUpdateStartedMessage> {
		public void Handle( LibraryUpdateStartedMessage message ) {
			if( EventAggregator != null ) {
				EventAggregator.Publish( new Events.LibraryUpdateStarted( message.LibraryId ));
			}
		}
	}

	public class LibraryUpdateCompletedMessage : IMessage {
		public long		LibraryId { get; set; }
	}

	public class LibraryUpdateCompletedMessageHandler : MessageHandlerBase, IMessageHandler<LibraryUpdateCompletedMessage> {
		public void Handle( LibraryUpdateCompletedMessage message ) {
			if( EventAggregator != null ) {
				EventAggregator.Publish( new Events.LibraryUpdateCompleted( message.LibraryId ));
			}
		}
	}

	public class DatabaseItemChangedMessage : IMessage {
		public long				ItemId { get; set; }
		public DbItemChanged	Change { get; set; }
	}

	public class DatabaseItemChangedMessageHandler : MessageHandlerBase, IMessageHandler<DatabaseItemChangedMessage> {
		public void Handle( DatabaseItemChangedMessage message ) {
			if( EventAggregator != null ) {
//				EventAggregator.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( message.ItemId, message.Change ));
			}
		}
	}
}
