using System;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	public class UiPlayQueueTrack : UiBase {
		private readonly PlayQueueTrack				mTrack;
		private readonly Action<UiPlayQueueTrack>	mMoveUp;
		private readonly Action<UiPlayQueueTrack>	mMoveDown;
		private readonly Action<UiPlayQueueTrack>	mDequeue;
		private readonly Action<UiPlayQueueTrack>	mPlayNow;
		private readonly Action<UiPlayQueueTrack>	mPlayFromHere;
		private readonly Action<UiPlayQueueTrack>	mDisplayInfo;

		public UiPlayQueueTrack( PlayQueueTrack track,
								 Action<UiPlayQueueTrack> onMoveUp, Action<UiPlayQueueTrack> onMoveDown, Action<UiPlayQueueTrack> onDisplayInfo,
								 Action<UiPlayQueueTrack> onDequeue, Action<UiPlayQueueTrack> onPlay, Action<UiPlayQueueTrack> onPlayFromHere ) {
			mTrack = track;
			mMoveUp = onMoveUp;
			mMoveDown = onMoveDown;
			mDisplayInfo = onDisplayInfo;
			mDequeue = onDequeue;
			mPlayNow = onPlay;
			mPlayFromHere = onPlayFromHere;

            if( track?.Track != null ) {
                UiIsFavorite = track.Track.IsFavorite;
                UiRating = track.Track.Rating;

                mTrack.PropertyChanged += ( sender, args ) => {
                    if(( args.PropertyName.Equals( "HasPlayed" )) ||
                       ( args.PropertyName.Equals( "IsPlaying" ))) {
                        RaisePropertyChanged( () => WillPlay );
                    }
                };
            }
		}

		public PlayQueueTrack QueuedTrack => ( mTrack );

	    public bool IsDeleting {
			get{ return( Get( () => IsDeleting )); }
			set{ Set( () => IsDeleting, value ); }
		}

        public bool WillPlay => (!QueuedTrack.HasPlayed && !QueuedTrack.IsPlaying);

	    public void Execute_DisplayInfo() {
	        mDisplayInfo?.Invoke( this );
	    }

	    public void Execute_MoveUp() {
	        mMoveUp?.Invoke( this );
	    }

	    public void Execute_MoveDown() {
	        mMoveDown?.Invoke( this );
	    }

	    public void Execute_Dequeue() {
	        mDequeue?.Invoke( this );
	    }

	    public void Execute_Play() {
	        mPlayNow?.Invoke( this );
	    }

	    public void Execute_Replay() {
			if( mTrack != null ) {
				mTrack.HasPlayed = false;
			}
		}

        public void Execute_SkipPlaying() {
            if( mTrack != null ) {
                mTrack.HasPlayed = true;
            }
        }

		public void Execute_PlayFromHere() {
		    mPlayFromHere?.Invoke( this );
		}
	}
}
