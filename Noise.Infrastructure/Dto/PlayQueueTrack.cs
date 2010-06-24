using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Dto {
	public class PlayQueueTrack : BindableObject {
		public	DbTrack		Track { get; private set; }
		public	StorageFile	File { get; private set; }
		private	bool		mIsPlaying;
		private	bool		mHasPlayed;

		public PlayQueueTrack( DbTrack track, StorageFile file ) {
			Track = track;
			File = file;
		}

		public bool IsPlaying {
			get{ return( mIsPlaying ); }
			set {
				mIsPlaying = value;

				NotifyOfPropertyChange( () => IsPlaying );
			}
		}

		public bool HasPlayed {
			get{ return( mHasPlayed ); }
			set {
				mHasPlayed = value;

				NotifyOfPropertyChange( () => HasPlayed );
			}
		}
	}
}
