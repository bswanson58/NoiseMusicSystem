using System.Diagnostics;
using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Dto {
	public enum eStrategySource {
		PlayStrategy,
		ExhaustedStrategy,
		User
	}

	[DebuggerDisplay("Track = {Name}")]
	public class PlayQueueTrack : BindableObject {
		public	DbTrack				Track { get; private set; }
		public	DbAlbum				Album { get; private set; }
		public	DbArtist			Artist { get; private set; }
		public	DbInternetStream	Stream { get; private set; }
		public	StreamInfo			StreamInfo { get; set; }
		public	StorageFile			File { get; private set; }
		public	string				FilePath { get; private set; }
		public	double				PercentPlayed { get; set; }
		public	eStrategySource		StrategySource { get; private set; }
		private	bool				mIsPlaying;
		private	bool				mHasPlayed;

		public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, StorageFile file, string filePath, eStrategySource strategySource ) :
			this( artist, album, track, file, filePath ) {
			StrategySource = strategySource;
		}

		public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, StorageFile file, string filePath ) {
			Artist = artist;
			Album = album;
			Track = track;
			File = file;
			FilePath = filePath;

			StrategySource = eStrategySource.User;
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

		public bool IsStrategyQueued {
			get{ return( StrategySource != eStrategySource.User ); }
		}

		public string Name {
			get{ return( Track != null ? Track.Name : Stream != null ? Stream.Name : "" ); }
		}
	}
}
