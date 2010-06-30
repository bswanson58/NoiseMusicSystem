using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class PlayQueueViewModel {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private readonly ObservableCollectionEx<PlayQueueTrack>	mPlayQueue;

		public PlayQueueViewModel() {
			mPlayQueue = new ObservableCollectionEx<PlayQueueTrack>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mEventAggregator.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
			}
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
