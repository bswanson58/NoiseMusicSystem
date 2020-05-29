using System;
using Noise.Infrastructure.Dto;
using Prism.Commands;

namespace Noise.UI.Dto {
	public class UiPlayQueueTrack : UiBase {
		private readonly PlayQueueTrack				mTrack;
		private readonly Action<UiPlayQueueTrack>	mMoveUp;
		private readonly Action<UiPlayQueueTrack>	mMoveDown;
		private readonly Action<UiPlayQueueTrack>	mDequeue;
		private readonly Action<UiPlayQueueTrack>	mPlayNow;
		private readonly Action<UiPlayQueueTrack>	mPlayFromHere;
		private readonly Action<UiPlayQueueTrack>	mDisplayInfo;
        private readonly Action<UiPlayQueueTrack>   mPromoteSuggestion;

        public	bool								WillPlay => !QueuedTrack.HasPlayed && !QueuedTrack.IsPlaying;

		public	DelegateCommand						DisplayInfo { get; }
		public	DelegateCommand						MoveUp { get; }
		public	DelegateCommand						MoveDown { get; }
		public	DelegateCommand						Dequeue { get; }
		public	DelegateCommand						Play {  get; }
		public	DelegateCommand						Replay { get; }
		public	DelegateCommand						SkipPlaying { get; }
		public	DelegateCommand						PlayFromHere { get; }
		public	DelegateCommand						PromoteSuggestion { get; }

		public UiPlayQueueTrack( PlayQueueTrack track,
								 Action<UiPlayQueueTrack> onMoveUp, Action<UiPlayQueueTrack> onMoveDown, Action<UiPlayQueueTrack> onDisplayInfo,
								 Action<UiPlayQueueTrack> onDequeue, Action<UiPlayQueueTrack> onPlay, Action<UiPlayQueueTrack> onPlayFromHere, Action<UiPlayQueueTrack> onPromoteSuggestion ) {
			mTrack = track;
			mMoveUp = onMoveUp;
			mMoveDown = onMoveDown;
			mDisplayInfo = onDisplayInfo;
			mDequeue = onDequeue;
			mPlayNow = onPlay;
			mPlayFromHere = onPlayFromHere;
            mPromoteSuggestion = onPromoteSuggestion;

            if( track?.Track != null ) {
				SetRatings( track.Track.IsFavorite, track.Track.Rating );

                mTrack.PropertyChanged += ( sender, args ) => {
                    if(( args.PropertyName.Equals( "HasPlayed" )) ||
                       ( args.PropertyName.Equals( "IsPlaying" ))) {
                        RaisePropertyChanged( () => WillPlay );
                    }
                };
            }

			DisplayInfo = new DelegateCommand( OnDisplayInfo );
			MoveUp = new DelegateCommand( OnMoveUp );
			MoveDown = new DelegateCommand( OnMoveDown );
			Dequeue = new DelegateCommand( OnDequeue );
			Play = new DelegateCommand( OnPlay );
			Replay = new DelegateCommand( OnReplay );
			SkipPlaying = new DelegateCommand( OnSkipPlaying );
			PlayFromHere = new DelegateCommand( OnPlayFromHere );
			PromoteSuggestion = new DelegateCommand( OnPromoteSuggestion );
		}

		public PlayQueueTrack   QueuedTrack => ( mTrack );
        public bool             IsStrategyQueued => QueuedTrack.IsStrategyQueued;

        public void NotifyPlayStrategyChanged() {
            RaisePropertyChanged( () => IsStrategyQueued );
        }
	    public bool IsDeleting {
			get{ return( Get( () => IsDeleting )); }
			set{ Set( () => IsDeleting, value ); }
		}


	    private void OnDisplayInfo() {
	        mDisplayInfo?.Invoke( this );
	    }

	    private void OnMoveUp() {
	        mMoveUp?.Invoke( this );
	    }

	    private void OnMoveDown() {
	        mMoveDown?.Invoke( this );
	    }

	    private void OnDequeue() {
	        mDequeue?.Invoke( this );
	    }

	    private void OnPlay() {
	        mPlayNow?.Invoke( this );
	    }

	    private void OnReplay() {
			if( mTrack != null ) {
				mTrack.HasPlayed = false;
			}
		}

        private void OnSkipPlaying() {
            if( mTrack != null ) {
                mTrack.HasPlayed = true;
            }
        }

		private void OnPlayFromHere() {
		    mPlayFromHere?.Invoke( this );
		}

        private void OnPromoteSuggestion() {
            mPromoteSuggestion?.Invoke( this );
        }
	}
}
