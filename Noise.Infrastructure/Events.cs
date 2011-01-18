using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure {
	public class Events {
		public class ArtistFocusRequested : CompositePresentationEvent<DbArtist> { }
		public class AlbumFocusRequested : CompositePresentationEvent<DbAlbum> { }

		public class ArtistContentUpdated : CompositePresentationEvent<DbArtist> { }

		public class PlayQueueChanged : CompositePresentationEvent<IPlayQueue> { }
		public class PlayHistoryChanged : CompositePresentationEvent<IPlayHistory> { }
		public class PlayListChanged : CompositePresentationEvent<IPlayListMgr> { }
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

		public class WebsiteRequest : CompositePresentationEvent<string> { }

		public class LibraryUpdateStarted : CompositePresentationEvent<long> { }
		public class LibraryUpdateCompleted : CompositePresentationEvent<long> { }

		public class SimilarSongSearchRequest : CompositePresentationEvent<long> { }
		public class SongLyricsRequest : CompositePresentationEvent<LyricsInfo> { }
		public class SongLyricsInfo : CompositePresentationEvent<LyricsInfo> { }

		public class SystemShutdown : CompositePresentationEvent<object> { }
	}
}
