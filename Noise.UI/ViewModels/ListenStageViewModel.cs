using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class ListenStageViewModel : ViewModelBase {
		private readonly IEventAggregator	mEvents;

		public ListenStageViewModel( IEventAggregator eventAggregator ) {
			mEvents = eventAggregator;

			mEvents.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnTrackStarted );
		}

		private static void OnTrackStarted( PlayQueueTrack track ) {
			
		}
	}
}
