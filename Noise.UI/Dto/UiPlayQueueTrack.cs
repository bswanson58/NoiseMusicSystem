using System;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiPlayQueueTrack : AutomaticCommandBase {
		private readonly PlayQueueTrack				mTrack;
		private bool								mIsFavorite;
		private Int16								mRating;
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
				mIsFavorite = track.Track.IsFavorite;
				mRating = track.Track.Rating;
			}
		}

		public PlayQueueTrack QueuedTrack {
			get{ return( mTrack ); }
		}

		public bool IsFavorite {
			get{ return( mIsFavorite ); }
			set{ mIsFavorite = value; }
		}

		public Int16 Rating {
			get{ return( mRating ); }
			set{ mRating = value; }
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

		public void Execute_PlayFromHere() {
			if( mPlayFromHere != null ) {
				mPlayFromHere( this );
			}	
		}
	}
}
