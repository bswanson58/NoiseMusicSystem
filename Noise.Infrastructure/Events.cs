using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;

namespace Noise.Infrastructure {
	public class Events {
		public class ArtistFocusRequested {
			public long	ArtistId { get; }

			public ArtistFocusRequested( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistListFocusRequested {
			public List<string>	ArtistList { get; }

			public ArtistListFocusRequested( IEnumerable<string> artistList ) {
				ArtistList = new List<string>( artistList );
			}
		}

		public class AlbumFocusRequested {
			public long	ArtistId { get; }
			public long	AlbumId { get; }

			public AlbumFocusRequested( long artistId, long albumId ) {
				ArtistId = artistId;
				AlbumId = albumId;
			}

			public AlbumFocusRequested( DbAlbum album ) :
				this( album.Artist, album.DbId ) { }
		}

		public class GenreFocusRequested {
			public string	Genre { get; }

			public GenreFocusRequested( string genre ) {
				Genre = genre;
            }
        }

        public class TagFocusRequested {
            public DbTag    Tag {  get; }

            public TagFocusRequested( DbTag tag) {
                Tag = tag;
            }
        }

		public class ArtistContentRequest {
			public long	ArtistId { get; }

			public ArtistContentRequest( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistMetadataUpdated {
			public string	ArtistName { get; }

			public ArtistMetadataUpdated( string artistName ) {
				ArtistName = artistName;
			}
		}

		public class ArtistContentUpdated {
			public long	ArtistId { get; }

			public ArtistContentUpdated( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistAdded {
			public	long	ArtistId { get; }

			public ArtistAdded( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistRemoved {
			public	long	ArtistId { get; }

			public ArtistRemoved( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistUserUpdate {
			public long		ArtistId { get; }

			public ArtistUserUpdate( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistViewed {
			public	long	ArtistId { get; }

			public ArtistViewed( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class ArtistPlayed {
			public	long	ArtistId { get; }

			public ArtistPlayed( long artistId ) {
				ArtistId = artistId;
			}
		}

		public class AlbumAdded {
			public	long	AlbumId { get; }

			public AlbumAdded( long albumId ) {
				AlbumId = albumId;
			}
		}

		public class AlbumStructureChanged {
            public	long	AlbumId { get; }

            public AlbumStructureChanged( long albumId ) {
                AlbumId = albumId;
            }
        }

		public class AlbumRemoved {
			public	long	AlbumId { get; }

			public AlbumRemoved( long albumId ) {
				AlbumId = albumId;
			}
		}

		public class AlbumUserUpdate {
			public long		AlbumId { get; }

			public AlbumUserUpdate( long albumId ) {
				AlbumId = albumId;
			}
		}

		public class TrackUserUpdate {
			public DbTrack	Track {  get; }

			public TrackUserUpdate( DbTrack track ) {
				Track = track;
			}
		}

		public class PlayQueueChanged {
			public IPlayQueue	PlayQueue { get; }

			public PlayQueueChanged( IPlayQueue playQueue ) {
				PlayQueue = playQueue;
			}
		}

		public class PlayHistoryChanged {
			public IPlayHistory	PlayHistory { get; }

			public PlayHistoryChanged( IPlayHistory playHistory ) {
				PlayHistory = playHistory;
			}
		}

		public class PlayListChanged {
			public DbPlayList	PlayList { get; }

			public PlayListChanged( DbPlayList playList ) {
				PlayList = playList;
			}
		}

		public class PlayListUserUpdate {
			public long		PlayListId	{ get; }

			public PlayListUserUpdate( long playListId ) {
				PlayListId = playListId;
			}
		}

		public class PlayQueuedTrackRequest {
			public PlayQueueTrack	QueuedTrack { get; }

			public PlayQueuedTrackRequest( PlayQueueTrack track ) {
				QueuedTrack = track;
			}
		}

		public class AlbumQueued {
			public DbAlbum		QueuedAlbum { get; }

			public AlbumQueued( DbAlbum album ) {
				QueuedAlbum = album;
			}
		}

		public class TrackQueued {
			public DbTrack QueuedTrack { get; }

			public TrackQueued( DbTrack track ) {
				QueuedTrack = track;
			}
		}

		public class PlayArtistTracksRandom {
			public long		ArtistId { get; }

			public PlayArtistTracksRandom( long artistId ) {
				ArtistId = artistId;
			} 
		}

		public class PlayAlbumTracksRandom {
			public	IEnumerable<DbAlbum>	AlbumList { get; }

			public PlayAlbumTracksRandom( IEnumerable<DbAlbum> albumList ) {
				AlbumList = albumList;
			} 
		}

		public class PlaybackStatusChanged {
			public ePlaybackStatus	Status { get; }

			public PlaybackStatusChanged( ePlaybackStatus newStatus ) {
				Status = newStatus;
			}
		}

		public class PlaybackTrackStarted {
			public PlayQueueTrack	Track { get; }

			public PlaybackTrackStarted( PlayQueueTrack track ) {
				Track = track;
			}
		}
        public class PlaybackStopped { }

		public class PlaybackTrackUpdated {
			public PlayQueueTrack	Track { get; }

			public PlaybackTrackUpdated( PlayQueueTrack track ) {
				Track = track;
			}
		}

		public class PlaybackTrackChanged { }
		public class PlaybackInfoChanged { }

		public class AudioParametersChanged { }

		public class WindowLayoutRequest {
			public string	LayoutName { get; }

			public WindowLayoutRequest( string layoutName ) {
				LayoutName = layoutName;
			}
		}

		public class ExternalPlayerSwitch { }
		public class ExtendedPlayerRequest { }
		public class StandardPlayerRequest { }

		public class LaunchRequest {
			public string	Target { get; }

			public LaunchRequest( string target ) {
				Target = target;
			}
		}

		public class UrlLaunchRequest {
			public string	Url { get; }

			public UrlLaunchRequest( string url ) {
				Url = url;
			}
		}

		public class LibraryBackupPressure {
			public	uint	PressureAdded {  get; }
			public	string	PressureSource { get; }

			public LibraryBackupPressure( uint pressure, string source ) {
				PressureAdded = pressure;
				PressureSource = source;
            }
        }

		public class LibraryBackupPressureThreshold {
			public enum ThresholdLevel {  Exceeded, Cleared }

			public ThresholdLevel	Threshold { get; }

			public LibraryBackupPressureThreshold( ThresholdLevel value ) {
				Threshold = value;
            }
		}

		public class LibraryUpdateStarted {
			public long	LibraryId { get; }

			public LibraryUpdateStarted( long libraryId ) {
				LibraryId = libraryId;
			}
		}

		public class LibraryUpdateCompleted {
			public	DatabaseChangeSummary	Summary { get; }

			public LibraryUpdateCompleted( DatabaseChangeSummary summary ) {
				Summary = summary;
			}
		}

		public class DatabaseStatisticsUpdated {
			public IDatabaseStatistics DatabaseStatistics {  get; }

			public DatabaseStatisticsUpdated( IDatabaseStatistics statistics ) {
				DatabaseStatistics = statistics;
			}
		}

		public class SongLyricsRequest {
			public LyricsInfo	LyricsInfo { get; }

			public SongLyricsRequest( LyricsInfo info ) {
				LyricsInfo = info;
			}
		}

		public class SongLyricsInfo {
			public LyricsInfo	LyricsInfo { get; }

			public SongLyricsInfo( LyricsInfo info ) {
				LyricsInfo = info;
			}
		}

		public class TimeExplorerAlbumFocus {
			public	IEnumerable<DbAlbum>	AlbumList { get; }

			public TimeExplorerAlbumFocus( IEnumerable<DbAlbum> albumList ) {
				AlbumList = new List<DbAlbum>( albumList );
			} 
		}

		public class BalloonPopupOpened {
			public string	ViewName { get; }

			public BalloonPopupOpened( string viewName ) {
				ViewName = viewName;
			}
		}

		public class ViewDisplayRequest {
			public string	ViewName { get; }
			public bool		ViewWasOpened { get; set; }
			 
			public ViewDisplayRequest( string viewName ) {
				ViewName = viewName;
			} 
		}

		public class NoiseSystemReady {
			public INoiseManager	NoiseManager { get; }
			public bool				WasInitialized { get; }

			public NoiseSystemReady( INoiseManager noiseManager, bool wasInitialized ) {
				NoiseManager = noiseManager;
				WasInitialized = wasInitialized;
			}
		}

		public class LibraryConfigurationLoaded {
			public	ILibraryConfiguration	LibraryConfiguration { get; }

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
			public GlobalUserEventArgs	UserEvent { get; }

			public GlobalUserEvent( GlobalUserEventArgs args ) {
				UserEvent = args;
			}
		}

		public enum StatusEventType {
			General,
			Speech
		}

		public class StatusEvent {
			public StatusEventType	StatusType { get; }
			public	string			Message { get; }
			public	bool			ExtendDisplay { get; set; }

			public StatusEvent( string message ) :
				this( message, StatusEventType.General ) { }

			public StatusEvent( string message, StatusEventType statusType ) {
				Message = message;
				StatusType = statusType;
			}
		}

		public class RemoteTransportUpdate {
			public RoTransportState	TransportState {get; }

			public RemoteTransportUpdate( RoTransportState transportState ) {
				TransportState = transportState;
			}
		}

        public class UserTagsChanged { }

		public class LibraryUserState {
			public	bool						IsRestoring { get; set; }
			public	Dictionary<string, object>	State { get; }

			public LibraryUserState() {
				State = new Dictionary<string, object>();
            }
        }
	}
}
