using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class PlayQueueControlViewModel : AutomaticCommandBase,
                                             IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackTrackStarted> {
		private readonly IEventAggregator   mEventAggregator;
		private readonly IPlayQueue			mPlayQueue;
		private TimeSpan					mTotalTime;
		private TimeSpan					mRemainingTime;

        public PlayQueueControlViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue ) {
            mPlayQueue = playQueue;
            mEventAggregator = eventAggregator;

			mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.PlayQueueChanged eventArgs ) {
			UpdatePlayTimes();

			PlayQueueChangedFlag++;
		}

        public void Handle( Events.PlaybackTrackStarted args ) {
            UpdatePlayTimes();

            RaiseCanExecuteChangedEvent( "CanExecute_ClearPlayed" );
        }

        private void UpdatePlayTimes() {
			mTotalTime = new TimeSpan();
			mRemainingTime = new TimeSpan();

			foreach( var track in mPlayQueue.PlayList ) {
				var	trackTime = track.Track != null ? track.Track.Duration : new TimeSpan();

				mTotalTime = mTotalTime.Add( trackTime );

				if((!track.HasPlayed ) ||
				   ( track.IsPlaying )) {
					mRemainingTime = mRemainingTime.Add( trackTime );
				}
			}

			RaisePropertyChanged( () => TotalTime );
			RaisePropertyChanged( () => RemainingTime );
        }

		public int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public TimeSpan TotalTime {
			get{ return( mTotalTime ); }
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public TimeSpan RemainingTime {
			get{ return( mRemainingTime ); }
		}

		public void Execute_ClearQueue( object sender ) {
			mPlayQueue.ClearQueue();
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearQueue( object sender ) {
			return(!mPlayQueue.IsQueueEmpty );
		}

		public void Execute_ClearPlayed() {
			mPlayQueue.RemovePlayedTracks();
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearPlayed() {
			return( mPlayQueue.PlayedTrackCount > 0 );
		}
    }
}
