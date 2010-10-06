using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Dto {
	public class PlayQueueTrack : BindableObject {
		public	DbTrack				Track { get; private set; }
		public	DbAlbum				Album { get; private set; }
		public	DbArtist			Artist { get; private set; }
		public	DbInternetStream	Stream { get; private set; }
		public	StreamInfo			StreamInfo { get; set; }
		public	StorageFile			File { get; private set; }
		public	double				PercentPlayed { get; set; }
		public	bool				IsStrategyQueued { get; private set; }
		private	bool				mIsPlaying;
		private	bool				mHasPlayed;

		public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, StorageFile file, bool strategyRequest ) :
			this( artist, album, track, file ) {
			IsStrategyQueued = strategyRequest;
		}

		public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, StorageFile file ) {
			Artist = artist;
			Album = album;
			Track = track;
			File = file;
		}

		public void UpdateTrack( DbTrack track ) {
			Track = track;
		}

		public PlayQueueTrack( DbInternetStream stream ) {
			Stream = stream;
		}

		public bool IsStream {
			get{ return( Stream != null ); }
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
