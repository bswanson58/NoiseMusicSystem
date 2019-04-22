using System;
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
		public	DbTrack						Track { get; private set; }
		public	DbAlbum						Album { get; private set; }
		public	DbArtist					Artist { get; private set; }
		public	DbInternetStream			Stream { get; private set; }
		public	StreamInfo					StreamInfo { get; set; }
		public	double						PercentPlayed { get; set; }
		public	eStrategySource				StrategySource { get; private set; }
		public	long						Uid { get; private set; }
		private	bool						mIsFaulted;
		private	bool						mIsPlaying;
		private	bool						mHasPlayed;
		private readonly Lazy<string>		mFilePath;
		private readonly Lazy<StorageFile>	mFile; 

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

		public string FilePath {
			get{ return( mFilePath.Value ); }
		}

		public StorageFile File {
			get{ return( mFile.Value ); }
		}

		public bool IsStream {
			get{ return( Stream != null ); }
		}

		public bool IsPlaying {
			get{ return( mIsPlaying ); }
			set {
				mIsPlaying = value;

				RaisePropertyChanged( () => IsPlaying );
				RaisePropertyChanged( () => HasPlayed );
			}
		}

		public bool HasPlayed {
			get{ return( mHasPlayed && !IsPlaying ); }
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

		public string AlbumName {
			get {
				var retValue = "";

				if((!IsStream ) &&
				   ( Album != null ) &&
				   ( Artist != null )) {
					retValue = string.Format( "{0}/{1}", Artist.Name, Album.Name );
				}

				return( retValue );
			}
		}

		public override string ToString() {
			var title = IsStream ? string.Format( "Stream: {0}", Stream.Name ) :
								   string.Format( "Track \"{0}\" Id:{1}, Artist \"{2}\" Id:{3}, Album \"{4}\" Id:{5}",
													Track.Name, Track.DbId, Artist.Name, Artist.DbId, Album.Name, Album.DbId );
			return( string.Format( "PlayTrack Id:{0}, Strategy:{1}, {2}", Uid, StrategySource, title ));
		}
	}
}
