using Microsoft.Practices.Composite.Presentation.Events;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure {
	public class Events {
		public class ExplorerItemSelected : CompositePresentationEvent<object> { }
		public class TrackSelected : CompositePresentationEvent<DbTrack> { }

		public class PlayQueueChanged : CompositePresentationEvent<IPlayQueue> { }

		public class AudioPlayStatusChanged : CompositePresentationEvent<int> { }
	}
}
