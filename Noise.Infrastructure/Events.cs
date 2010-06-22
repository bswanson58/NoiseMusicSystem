using Microsoft.Practices.Composite.Presentation.Events;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure {
	public class Events {
		public class ExplorerItemSelected : CompositePresentationEvent<object> { }
		public class TrackSelected : CompositePresentationEvent<DbTrack> { }
	}
}
