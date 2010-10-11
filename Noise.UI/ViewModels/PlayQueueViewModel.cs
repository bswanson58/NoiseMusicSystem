using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class PlayQueueViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private INoiseManager		mNoiseManager;
		private IEventAggregator	mEventAggregator;
		private int					mPlayingIndex;
		private readonly ObservableCollectionEx<PlayQueueTrack>	mPlayQueue;

		public PlayQueueViewModel() {
			mPlayQueue = new ObservableCollectionEx<PlayQueueTrack>();
			mPlayingIndex = -1;
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();
				mEventAggregator.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
				mEventAggregator.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnTrackStarted );

				LoadPlayQueue();
			}
		}

		public ObservableCollectionEx<PlayQueueTrack> QueueList {
			get{ return( mPlayQueue ); }
		}

		private void OnPlayQueueChanged( IPlayQueue playQueue ) {
			Invoke( LoadPlayQueue );
		}

		private void LoadPlayQueue() {
			mPlayQueue.Clear();
			mPlayQueue.AddRange( mNoiseManager.PlayQueue.PlayList );
		}

		private void OnTrackStarted( PlayQueueTrack track ) {
			var index = 0;

			mPlayingIndex = -1;

			foreach( var item in mPlayQueue ) {
				if( item.IsPlaying ) {
					mPlayingIndex = index;

					break;
				}

				index++;
			}

			RaisePropertyChanged( () => PlayingIndex );
		}

		public int PlayingIndex {
			get{ return( mPlayingIndex ); }
		}
	}
}
