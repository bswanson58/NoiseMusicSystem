﻿using Microsoft.Practices.Composite.Presentation.Events;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure {
	public class Events {
		public class ExplorerItemSelected : CompositePresentationEvent<object> { }
		public class TrackSelected : CompositePresentationEvent<DbTrack> { }

		public class ArtistFocusRequested : CompositePresentationEvent<DbArtist> { }
		public class AlbumFocusRequested : CompositePresentationEvent<DbAlbum> { }

		public class ArtistContentUpdated : CompositePresentationEvent<DbArtist> { }

		public class PlayQueueChanged : CompositePresentationEvent<IPlayQueue> { }
		public class PlayHistoryChanged : CompositePresentationEvent<IPlayHistory> { }
		public class PlayListChanged : CompositePresentationEvent<IPlayListMgr> { }
		public class PlayRequested : CompositePresentationEvent<PlayQueueTrack> { }

		public class AudioPlayStatusChanged : CompositePresentationEvent<int> { }
		public class AudioPlayStreamInfo : CompositePresentationEvent<StreamInfo> { }

		public class AlbumPlayRequested : CompositePresentationEvent<DbAlbum> { }
		public class TrackPlayRequested : CompositePresentationEvent<DbTrack> { }
		public class StreamPlayRequested : CompositePresentationEvent<DbInternetStream> { }

		public class DatabaseChanged : CompositePresentationEvent<DatabaseChangeSummary> { }

		public class PlaybackStatusChanged : CompositePresentationEvent<ePlaybackStatus> { }
		public class PlaybackTrackChanged : CompositePresentationEvent<object> { }
		public class PlaybackInfoChanged : CompositePresentationEvent<object> { }
		public class PlaybackTrackStarted : CompositePresentationEvent<PlayQueueTrack> { }

		public class WindowLayoutRequest : CompositePresentationEvent<string> { }

		public class DatabaseItemChanged : CompositePresentationEvent<DbItemChangedArgs> { }

		public class WebsiteRequest : CompositePresentationEvent<string> { }
	}
}
