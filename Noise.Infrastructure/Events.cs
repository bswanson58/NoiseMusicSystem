using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure {
	public class Events {
		public class ArtistFocusRequested {
			public long	ArtistId { get; private set; }

			public ArtistFocusRequested( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class AlbumFocusRequested {
			public long	ArtistId { get; private set; }
			public long	AlbumId { get; private set; }

			private AlbumFocusRequested( long artistId, long albumId ) {
				ArtistId = artistId;
				AlbumId = albumId;
			}

			public AlbumFocusRequested( DbAlbum album ) :
				this( album.Artist, album.DbId ) { }
		}

		public class ArtistContentRequest {
			public long	ArtistId { get; private set; }

			public ArtistContentRequest( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistContentUpdated {
			public long	ArtistId { get; private set; }

			public ArtistContentUpdated( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class PlayQueueChanged {
			public IPlayQueue	PlayQueue { get; private set; }

			public PlayQueueChanged( IPlayQueue playQueue ) {
				PlayQueue = playQueue;
			}
		}

		public class PlayHistoryChanged {
			public IPlayHistory	PlayHistory { get; private set; }

			public PlayHistoryChanged( IPlayHistory playHistory ) {
				PlayHistory = playHistory;
			}
		}

		public class PlayListChanged {
			public DbPlayList	PlayList { get; private set; }

			public PlayListChanged( DbPlayList playList ) {
				PlayList = playList;
			}
		}

		public class PlayQueuedTrackRequest {
			public PlayQueueTrack	QueuedTrack { get; private set; }

			public PlayQueuedTrackRequest( PlayQueueTrack track ) {
				QueuedTrack = track;
			}
		}

		public class DatabaseItemChanged {
			public DbItemChangedArgs	ItemChangedArgs { get; private set; }

			public DatabaseItemChanged( DbItemChangedArgs args ) {
				ItemChangedArgs = args;
			}
		}

		public class PlaybackStatusChanged {
			public ePlaybackStatus	Status { get; private set; }

			public PlaybackStatusChanged( ePlaybackStatus newStatus ) {
				Status = newStatus;
			}
		}

		public class PlaybackTrackStarted {
			public PlayQueueTrack	Track { get; private set; }

			public PlaybackTrackStarted( PlayQueueTrack track ) {
				Track = track;
			}
		}

		public class PlaybackTrackChanged { }
		public class PlaybackInfoChanged { }

		public class WindowLayoutRequest {
			public string	LayoutName { get; private set; }

			public WindowLayoutRequest( string layoutName ) {
				LayoutName = layoutName;
			}
		}

		public class ExternalPlayerSwitch { }
		public class ExtendedPlayerRequest { }
		public class StandardPlayerRequest { }

		public class LaunchRequest {
			public string	Target { get; private set; }

			public LaunchRequest( string target ) {
				Target = target;
			}
		}

		public class UrlLaunchRequest {
			public string	Url { get; private set; }

			public UrlLaunchRequest( string url ) {
				Url = url;
			}
		}

		public class NavigationRequest {
			public	string	TargetView { get; private set; }

			public NavigationRequest( string target ) {
				TargetView = target;
			}
		}

		public class LibraryUpdateStarted {
			public long	LibraryId { get; private set; }

			public LibraryUpdateStarted( long libraryId ) {
				LibraryId = libraryId;
			}
		}

		public class LibraryUpdateCompleted {
			public	long	LibraryId { get; private set; }

			public LibraryUpdateCompleted( long libraryId ) {
				LibraryId = libraryId;
			}
		}

		public class SimilarSongSearchRequest : CompositePresentationEvent<long> { }
		public class SongLyricsRequest : CompositePresentationEvent<LyricsInfo> { }
		public class SongLyricsInfo : CompositePresentationEvent<LyricsInfo> { }

		public class BalloonPopupOpened : CompositePresentationEvent<object> { }

		public class SystemConfigurationChanged : CompositePresentationEvent<object> { }
		public class SystemShutdown : CompositePresentationEvent<object> { }

		public class GlobalUserEvent : CompositePresentationEvent<GlobalUserEventArgs> { }
	}
}
