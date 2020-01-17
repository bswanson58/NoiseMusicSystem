using System;
using System.Diagnostics;
using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Dto {
	public enum eStrategySource {
		PlayStrategy,
		ExhaustedStrategy,
		User
	}

	[DebuggerDisplay("Track = {" + nameof(Name) + "}")]
	public class PlayQueueTrack : BindableObject {
		public	DbTrack						Track { get; private set; }
		public	DbAlbum						Album { get; }
		public	DbArtist					Artist { get; }
		public	DbInternetStream			Stream { get; }
		public	StreamInfo					StreamInfo { get; set; }
		public	double						PercentPlayed { get; set; }
		public	eStrategySource				StrategySource { get; private set; }
		public	long						Uid { get; }
		private	bool						mIsFaulted;
		private	bool						mIsPlaying;
		private	bool						mHasPlayed;
		private readonly Lazy<string>		mFilePath;
		private readonly Lazy<StorageFile>	mFile; 

        public	string						FilePath => mFilePath.Value;
        public	StorageFile					File => mFile.Value;
        public	bool						IsStream => Stream != null;
		public	bool						IsTrack => Track != null;
        public	bool						IsStrategyQueued => StrategySource != eStrategySource.User;
		public	string						Name => Track != null ? Track.Name : Stream != null ? Stream.Name : String.Empty;

        public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, Func<DbTrack, StorageFile> fileProvider, Func<StorageFile, string> pathProvider , eStrategySource strategySource ) :
			this( artist, album, track, fileProvider, pathProvider ) {
			StrategySource = strategySource;
		}

		public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, Func<DbTrack, StorageFile> fileProvider, Func<StorageFile, string> pathProvider ) {
			Artist = artist;
			Album = album;
			Track = track;

			mFile = new Lazy<StorageFile>( () => fileProvider( Track ));
			mFilePath = new Lazy<string>( () => pathProvider( File ));

			StrategySource = eStrategySource.User;

			Uid = DatabaseIdentityProvider.Current.NewIdentityAsLong();
		}

		public PlayQueueTrack( DbArtist artist, DbAlbum album, DbTrack track, StorageFile file, string path ) {
			Artist = artist;
			Album = album;
			Track = track;

			mFile = new Lazy<StorageFile>( () => file );
			mFilePath = new Lazy<string>( () => path );

			StrategySource = eStrategySource.User;

			Uid = DatabaseIdentityProvider.Current.NewIdentityAsLong();
		}

		public void UpdateTrack( DbTrack track ) {
			Track = track;
		}

		public PlayQueueTrack( DbInternetStream stream ) {
			Stream = stream;

			StrategySource = eStrategySource.User;
		}

        public void PromoteStrategy() {
            StrategySource = eStrategySource.User;
        }

        public bool IsPlaying {
			get => ( mIsPlaying );
            set {
				mIsPlaying = value;

				RaisePropertyChanged( () => IsPlaying );
				RaisePropertyChanged( () => HasPlayed );
			}
		}

		public bool HasPlayed {
			get => ( mHasPlayed && !IsPlaying );
            set {
				mHasPlayed = value;

				RaisePropertyChanged( () => HasPlayed );
			}
		}

		public bool IsFaulted {
			get => ( mIsFaulted );
            set {
				mIsFaulted = value;

				RaisePropertyChanged( () => IsFaulted );
			}
		}

        public string FullName {
			get {
				var retValue = "";

				if( IsStream ) {
					if( Stream != null ) {
						retValue = string.IsNullOrWhiteSpace( Stream.Description ) ? Stream.Name : $"{Stream.Name} ({Stream.Description})";
					}
				}
				else {
					if( Track != null ) {
						if(( Album != null ) &&
						   ( Artist != null )) {
							retValue = $"{Track.Name} ({Artist.Name}/{Album.Name})";
						}
						else {
							retValue = Track.Name;
						}
					}
				}

				return( retValue );
			}
		}

		public string AlbumName {
			get {
				var retValue = "";

				if((!IsStream ) &&
				   ( Album != null ) &&
				   ( Artist != null )) {
					retValue = $"{Artist.Name}/{Album.Name}";
				}

				return( retValue );
			}
		}

		public override string ToString() {
			var title = IsStream ? $"Stream: {Stream.Name}" : $"Track \"{Track.Name}\" Id:{Track.DbId}, Artist \"{Artist.Name}\" Id:{Artist.DbId}, Album \"{Album.Name}\" Id:{Album.DbId}";

			return( $"PlayTrack Id:{Uid}, Strategy:{StrategySource}, {title}" );
		}
	}
}
