using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Dto {
	public class PlayQueueTrack : BindableObject {
		public	DbTrack		Track { get; private set; }
		public	DbAlbum		Album { get; private set; }
		public	DbArtist	Artist { get; private set; }
		public	StorageFile	File { get; private set; }
		public	double		PercentPlayed { get; set; }
		private	bool		mIsPlaying;
		private	bool		mHasPlayed;

		public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, StorageFile file ) {
			Artist = artist;
			Album = album;
			Track = track;
			File = file;
		}

		public bool IsPlaying {
			get{ return( mIsPlaying ); }
			set {
				mIsPlaying = value;

				RaisePropertyChanged( () => IsPlaying );
			}
		}

		public bool HasPlayed {
			get{ return( mHasPlayed ); }
			set {
				mHasPlayed = value;

				RaisePropertyChanged( () => HasPlayed );
			}
		}
	}
}
