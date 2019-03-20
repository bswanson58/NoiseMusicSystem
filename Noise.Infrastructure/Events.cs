using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;

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

        public class TagFocusRequested {
            public DbTag    Tag {  get; private set; }

            public TagFocusRequested( DbTag tag) {
                Tag = tag;
            }
        }

		public class ArtistContentRequest {
			public long	ArtistId { get; private set; }

			public ArtistContentRequest( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistMetadataUpdated {
			public string	ArtistName { get; private set; }

			public ArtistMetadataUpdated( string artistName ) {
				ArtistName = artistName;
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

		public class ArtistViewed {
			public	long	ArtistId { get; private set; }

			public ArtistViewed( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistPlayed {
			public	long	ArtistId { get; private set; }

			public ArtistPlayed( long artistId ) {
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
			public DbTrack	Track {  get; private set; }

			public TrackUserUpdate( DbTrack track ) {
				Track = track;
			}
		}

		public class PlayQueueChanged {
			public IPlayQueue	PlayQueue { get; private set; }

			public PlayQueueChanged( IPlayQueue playQueue ) {
				PlayQueue = playQueue;
			}
		}

		public class PlayExhaustedStrategyChanged {
			public ePlayExhaustedStrategy	ExhaustedStrategy { get; private set; }
			public IPlayStrategyParameters	StrategyParameters { get; private set; }

			public PlayExhaustedStrategyChanged( ePlayExhaustedStrategy strategy, IPlayStrategyParameters parameters ) {
				ExhaustedStrategy = strategy;
				StrategyParameters = parameters;
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

		public class AlbumQueued {
			public DbAlbum		QueuedAlbum { get; private set; }

			public AlbumQueued( DbAlbum album ) {
				QueuedAlbum = album;
			}
		}

		public class TrackQueued {
			public DbTrack QueuedTrack { get; private set; }

			public TrackQueued( DbTrack track ) {
				QueuedTrack = track;
			}
		}

		public class PlayArtistTracksRandom {
			public long		ArtistId { get; private set; }

			public PlayArtistTracksRandom( long artistId ) {
				ArtistId = artistId;
			} 
		}

		public class PlayAlbumTracksRandom {
			public	IEnumerable<DbAlbum>	AlbumList { get; private set; }

			public PlayAlbumTracksRandom( IEnumerable<DbAlbum> albumList ) {
				AlbumList = albumList;
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

		public class AudioParametersChanged { }

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
			public	DatabaseChangeSummary	Summary { get; private set; }

			public LibraryUpdateCompleted( DatabaseChangeSummary summary ) {
				Summary = summary;
			}
		}

		public class DatabaseStatisticsUpdated {
			public IDatabaseStatistics DatabaseStatistics {  get; private set; }

			public DatabaseStatisticsUpdated( IDatabaseStatistics statistics ) {
				DatabaseStatistics = statistics;
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

		public class TimeExplorerAlbumFocus {
			public	IEnumerable<DbAlbum>	AlbumList { get; private set; }

			public TimeExplorerAlbumFocus( IEnumerable<DbAlbum> albumList ) {
				AlbumList = new List<DbAlbum>( albumList );
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

		public class NoiseSystemReady {
			public INoiseManager	NoiseManager { get; private set; }
			public bool				WasInitialized { get; private set; }

			public NoiseSystemReady( INoiseManager noiseManager, bool wasInitialized ) {
				NoiseManager = noiseManager;
				WasInitialized = wasInitialized;
			}
		}

		public class LibraryConfigurationLoaded {
			public	ILibraryConfiguration	LibraryConfiguration { get; private set; }

			public LibraryConfigurationLoaded( ILibraryConfiguration libraryConfiguration ) {
				LibraryConfiguration = libraryConfiguration;
			}	
		}
		public class LibraryConfigurationChanged { }
		public class LibraryListChanged { }
		public class LibraryChanged { }
		public class LibraryBackupsChanged { }

		public class DatabaseOpened { }
		public class DatabaseClosing { }

		public class SystemInitialized { }
		public class SystemShutdown { }

		public class GlobalUserEvent {
			public GlobalUserEventArgs	UserEvent { get; private set; }

			public GlobalUserEvent( GlobalUserEventArgs args ) {
				UserEvent = args;
			}
		}

		public enum StatusEventType {
			General,
			Speech
		}

		public class StatusEvent {
			public StatusEventType	StatusType { get; private set; }
			public	string			Message { get; private set; }
			public	bool			ExtendDisplay { get; set; }

			public StatusEvent( string message ) :
				this( message, StatusEventType.General ) { }

			public StatusEvent( string message, StatusEventType statusType ) {
				Message = message;
				StatusType = statusType;
			}
		}

		public class RemoteTransportUpdate {
			public RoTransportState	TransportState {get; private set; }

			public RemoteTransportUpdate( RoTransportState transportState ) {
				TransportState = transportState;
			}
		}
	}
}
