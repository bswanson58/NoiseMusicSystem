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

			public AlbumFocusRequested( long artistId, long albumId ) {
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

		public class ArtistAdded {
			public	long	ArtistId { get; private set; }

			public ArtistAdded( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistRemoved {
			public	long	ArtistId { get; private set; }

			public ArtistRemoved( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistUserUpdate {
			public long		ArtistId { get; private set; }

			public ArtistUserUpdate( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class AlbumAdded {
			public	long	AlbumId { get; private set; }

			public AlbumAdded( long albumId ) {
				AlbumId = albumId;
			}
		}

		public class AlbumRemoved {
			public	long	AlbumId { get; private set; }

			public AlbumRemoved( long albumId ) {
				AlbumId = albumId;
			}
		}

		public class AlbumUserUpdate {
			public long		AlbumId { get; private set; }

			public AlbumUserUpdate( long albumId ) {
				AlbumId = albumId;
			}
		}

		public class TrackUserUpdate {
			public long		TrackId { get; private set; }

			public TrackUserUpdate( long trackId ) {
				TrackId = trackId;
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

		public class PlayListUserUpdate {
			public long		PlayListId	{ get; private set; }

			public PlayListUserUpdate( long playListId ) {
				PlayListId = playListId;
			}
		}

		public class PlayQueuedTrackRequest {
			public PlayQueueTrack	QueuedTrack { get; private set; }

			public PlayQueuedTrackRequest( PlayQueueTrack track ) {
				QueuedTrack = track;
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

		public class PlaybackTrackUpdated {
			public PlayQueueTrack	Track { get; private set; }

			public PlaybackTrackUpdated( PlayQueueTrack track ) {
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

		public class SimilarSongSearchRequest {
			public long		TrackId { get; private set; }

			public SimilarSongSearchRequest( long trackId ) {
				TrackId = trackId;
			}
		}

		public class SongLyricsRequest {
			public LyricsInfo	LyricsInfo { get; private set; }

			public SongLyricsRequest( LyricsInfo info ) {
				LyricsInfo = info;
			}
		}

		public class SongLyricsInfo {
			public LyricsInfo	LyricsInfo { get; private set; }

			public SongLyricsInfo( LyricsInfo info ) {
				LyricsInfo = info;
			}
		}

		public class BalloonPopupOpened {
			public string	ViewName { get; private set; }

			public BalloonPopupOpened( string viewName ) {
				ViewName = viewName;
			}
		}

		public class ViewDisplayRequest {
			public string	ViewName { get; private set; }
			public bool		ViewWasOpened { get; set; }
			 
			public ViewDisplayRequest( string viewName ) {
				ViewName = viewName;
			} 
		}

		public class LibraryConfigurationChanged { }
		public class LibraryChanged { }

		public class DatabaseOpened { }
		public class DatabaseClosing { }

		public class SystemShutdown { }

		public class GlobalUserEvent {
			public GlobalUserEventArgs	UserEvent { get; private set; }

			public GlobalUserEvent( GlobalUserEventArgs args ) {
				UserEvent = args;
			}
		}
	}
}
