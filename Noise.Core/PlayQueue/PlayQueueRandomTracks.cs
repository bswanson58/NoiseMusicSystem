using System.Collections.Generic;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueRandomTracks : IPlayQueueSupport,
										   IHandle<Events.PlayArtistTracksRandom>, IHandle<Events.PlayAlbumTracksRandom> {
		private const int							cTracksToQueue = 10;

		private readonly IEventAggregator			mEventAggregator;
		private readonly IArtistProvider			mArtistProvider;
		private readonly IRandomTrackSelector		mTrackSelector;
		private readonly ILogPlayStrategy			mLog;
		private TaskHandler<IEnumerable<DbTrack>>	mTrackQueueTaskHandler;
		private IPlayQueue							mPlayQueueMgr;

		public PlayQueueRandomTracks( IEventAggregator eventAggregator, IRandomTrackSelector randomTrackSelector, IArtistProvider artistProvider, ILogPlayStrategy log ) {
			mEventAggregator = eventAggregator;
			mTrackSelector = randomTrackSelector;
			mArtistProvider = artistProvider;
			mLog = log;
		}

		public bool Initialize( IPlayQueue playQueueMgr ) {
			mPlayQueueMgr = playQueueMgr;

			mEventAggregator.Subscribe( this );

			return( true );
		}

		public void Handle( Events.PlayArtistTracksRandom message ) {
			Condition.Requires( mPlayQueueMgr ).IsNotNull();

			var artist = mArtistProvider.GetArtist( message.ArtistId );

			if( artist != null ) {
				QueueArtistTracks( artist );
			}
		}

		public void Handle( Events.PlayAlbumTracksRandom message ) {
			Condition.Requires( mPlayQueueMgr ).IsNotNull();

			QueueAlbumTracks( message.AlbumList );
		}

		internal TaskHandler<IEnumerable<DbTrack>>  TrackQueueTaskHandler {
			get {
				if( mTrackQueueTaskHandler == null ) {
					Execute.OnUIThread( () => mTrackQueueTaskHandler = new TaskHandler<IEnumerable<DbTrack>> () );
				}

				return ( mTrackQueueTaskHandler );
			}

			set { mTrackQueueTaskHandler = value; }
		}


		private void QueueArtistTracks( DbArtist artist ) {
			TrackQueueTaskHandler.StartTask( () => mTrackSelector.SelectTracks( artist, track => !mPlayQueueMgr.IsTrackQueued( track ), cTracksToQueue ),
											 trackList  => mPlayQueueMgr.Add( trackList ),
											 error => mLog.LogException( "Queuing tracks for artist", error ));
		}

		private void QueueAlbumTracks( IEnumerable<DbAlbum> albumList ) {
			TrackQueueTaskHandler.StartTask( () => mTrackSelector.SelectTracks( albumList, track => !mPlayQueueMgr.IsTrackQueued( track ), cTracksToQueue ),
											 trackList => mPlayQueueMgr.Add( trackList ),
											 error => mLog.LogException( "Queuing tracks for album", error ));
		}
	}
}
