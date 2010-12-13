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
		private	bool				mIsFaulted;
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

			StrategySource = eStrategySource.User;
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

		public bool IsFaulted {
			get{ return( mIsFaulted ); }
			set {
				mIsFaulted = value;

				RaisePropertyChanged( () => IsFaulted );
			}
		}

		public bool IsStrategyQueued {
			get{ return( StrategySource != eStrategySource.User ); }
		}

		public string Name {
			get{ return( Track != null ? Track.Name : Stream != null ? Stream.Name : "" ); }
		}

		public string FullName {
			get {
				var retValue = "";

				if( IsStream ) {
					if( Stream != null ) {
						retValue = string.IsNullOrWhiteSpace( Stream.Description ) ? Stream.Name : string.Format( "{0} ({1})", Stream.Name, Stream.Description );
					}
				}
				else {
					if( Track != null ) {
						if(( Album != null ) &&
						   ( Artist != null )) {
							retValue = string.Format( "{0} ({1}/{2})", Track.Name, Artist.Name, Album.Name );
						}
						else {
							retValue = Track.Name;
						}
					}
				}

				return( retValue );
			}
		}
	}
}
