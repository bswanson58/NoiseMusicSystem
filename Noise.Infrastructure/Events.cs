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

		public class PlayRequested : CompositePresentationEvent<PlayQueueTrack> { }

		public class PlaybackStatusChanged : CompositePresentationEvent<ePlaybackStatus> { }
		public class PlaybackTrackChanged : CompositePresentationEvent<object> { }
		public class PlaybackInfoChanged : CompositePresentationEvent<object> { }
		public class PlaybackTrackStarted : CompositePresentationEvent<PlayQueueTrack> { }

		public class WindowLayoutRequest : CompositePresentationEvent<string> { }
		public class ExternalPlayerSwitch : CompositePresentationEvent<object> { }
		public class ExtendedPlayerRequest : CompositePresentationEvent<object> { }
		public class StandardPlayerRequest : CompositePresentationEvent<object> { }

		public class DatabaseItemChanged : CompositePresentationEvent<DbItemChangedArgs> { }

		public class LaunchRequest : CompositePresentationEvent<string > { }
		public class WebsiteRequest : CompositePresentationEvent<string> { }
		public class NavigationRequest : CompositePresentationEvent<NavigationRequestArgs> { }

		public class LibraryUpdateStarted : CompositePresentationEvent<long> { }
		public class LibraryUpdateCompleted : CompositePresentationEvent<long> { }

		public class SimilarSongSearchRequest : CompositePresentationEvent<long> { }
		public class SongLyricsRequest : CompositePresentationEvent<LyricsInfo> { }
		public class SongLyricsInfo : CompositePresentationEvent<LyricsInfo> { }

		public class BalloonPopupOpened : CompositePresentationEvent<object> { }

		public class SystemConfigurationChanged : CompositePresentationEvent<object> { }
		public class SystemShutdown : CompositePresentationEvent<object> { }

		public class GlobalUserEvent : CompositePresentationEvent<GlobalUserEventArgs> { }
	}
}
