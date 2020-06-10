using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class PlayQueueControlViewModel : AutomaticPropertyBase,
                                             IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackTrackStarted> {
		private readonly IPlayQueue			mPlayQueue;
		private TimeSpan					mTotalTime;
		private TimeSpan					mRemainingTime;

        public	TimeSpan					TotalTime => mTotalTime;
        public	TimeSpan					RemainingTime => mRemainingTime;

        public	DelegateCommand				ClearQueue { get; }
		public	DelegateCommand				ClearPlayed { get; }

        public PlayQueueControlViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue ) {
            mPlayQueue = playQueue;

			ClearQueue = new DelegateCommand( OnClearQueue, CanClearQueue );
			ClearPlayed = new DelegateCommand( OnClearPlayed, CanClearPlayed );

			eventAggregator.Subscribe( this );
        }

        public void Handle( Events.PlayQueueChanged eventArgs ) {
			UpdatePlayTimes();

			PlayQueueChangedFlag++;
		}

        public void Handle( Events.PlaybackTrackStarted args ) {
            UpdatePlayTimes();

			ClearPlayed.RaiseCanExecuteChanged();
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
			set {
                Set( () => PlayQueueChangedFlag, value  );

				RaisePropertyChanged( () => TotalTime );
				RaisePropertyChanged( () => RemainingTime );
				ClearPlayed.RaiseCanExecuteChanged();
				ClearQueue.RaiseCanExecuteChanged();
            }
		}


        private void OnClearQueue() {
			mPlayQueue.ClearQueue();
		}

		private bool CanClearQueue() {
			return(!mPlayQueue.IsQueueEmpty );
		}

		private void OnClearPlayed() {
			mPlayQueue.RemovePlayedTracks();
		}

		private bool CanClearPlayed() {
			return( mPlayQueue.PlayedTrackCount > 0 );
		}
    }
}
