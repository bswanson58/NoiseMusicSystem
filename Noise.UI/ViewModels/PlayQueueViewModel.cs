using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class PlayQueueViewModel {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEventAggregator;
		private readonly ObservableCollectionEx<PlayQueueTrack>	mPlayQueue;

		public PlayQueueViewModel( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();

			mPlayQueue = new ObservableCollectionEx<PlayQueueTrack>();

			mEventAggregator.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
		}

		public ObservableCollectionEx<PlayQueueTrack> QueueList {
			get{ return( mPlayQueue ); }
		}

		public void OnPlayQueueChanged( IPlayQueue playQueue ) {
			mPlayQueue.Clear();
			mPlayQueue.AddRange( playQueue.PlayList );
		}
	}
}
