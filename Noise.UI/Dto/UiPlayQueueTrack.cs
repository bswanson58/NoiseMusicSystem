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

			if(( track != null ) &&
			   ( track.Track != null )) {
				UiIsFavorite = track.Track.IsFavorite;
				UiRating = track.Track.Rating;
			}
		}

		public PlayQueueTrack QueuedTrack {
			get{ return( mTrack ); }
		}

		public bool IsDeleting {
			get{ return( Get( () => IsDeleting )); }
			set{ Set( () => IsDeleting, value ); }
		}

		public void Execute_DisplayInfo() {
			if( mDisplayInfo != null ) {
				mDisplayInfo( this );
			}
		}

		public void Execute_MoveUp() {
			if( mMoveUp != null ) {
				mMoveUp( this );
			}
		}

		public void Execute_MoveDown() {
			if( mMoveDown != null ) {
				mMoveDown( this );
			}
		}

		public void Execute_Dequeue() {
			if( mDequeue != null ) {
				mDequeue( this );
			}
		}

		public void Execute_Play() {
			if( mPlayNow != null ) {
				mPlayNow( this );
			}
		}

		public void Execute_Replay() {
			if( mTrack != null ) {
				mTrack.HasPlayed = false;
			}
		}

		public void Execute_PlayFromHere() {
			if( mPlayFromHere != null ) {
				mPlayFromHere( this );
			}	
		}
	}
}
