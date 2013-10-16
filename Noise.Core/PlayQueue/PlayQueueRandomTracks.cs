using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueRandomTracks : IPlayQueueSupport,
										   IHandle<Events.PlayArtistTracksRandom>, IHandle<Events.PlayAlbumTracksRandom> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private TaskHandler					mTrackQueueTaskHandler;
		private IPlayQueue					mPlayQueueMgr;

		public PlayQueueRandomTracks( IEventAggregator eventAggregator, IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

		public bool Initialize( IPlayQueue playQueueMgr ) {
			mPlayQueueMgr = playQueueMgr;

			mEventAggregator.Subscribe( this );

			return( true );
		}

		public void Handle( Events.PlayArtistTracksRandom message ) {
			Condition.Requires( mPlayQueueMgr ).IsNotNull();

			QueueTracks( () => BuildAlbumList( message.ArtistId ));
		}

		public void Handle( Events.PlayAlbumTracksRandom message ) {
			Condition.Requires( mPlayQueueMgr ).IsNotNull();

			QueueTracks( () => message.AlbumList );
		}

		internal TaskHandler TrackQueueTaskHandler {
			get {
				if( mTrackQueueTaskHandler == null ) {
					Execute.OnUIThread( () => mTrackQueueTaskHandler = new TaskHandler() );
				}

				return ( mTrackQueueTaskHandler );
			}

			set { mTrackQueueTaskHandler = value; }
		}


		private void QueueTracks( Func<IEnumerable<DbAlbum>> albumList ) {
			TrackQueueTaskHandler.StartTask( () => QueueFromAlbumList( albumList()),
											 () => { },
											 error => NoiseLogger.Current.LogException( "PlayQueueRandomTrack:QueueTracks", error ));
		}

		private void QueueFromAlbumList( IEnumerable<DbAlbum> albumList ) {
			var trackList = BuildTrackList( albumList );
			var queueTrackCount = 10;

			while(( queueTrackCount > 0 ) &&
			      ( trackList.Any())) {
				var selectedTrack = SelectTrackAtRandom( trackList );

				if( selectedTrack != null ) {
					if(!mPlayQueueMgr.IsTrackQueued( selectedTrack )) {
						mPlayQueueMgr.Add( selectedTrack );
						trackList.Remove( selectedTrack );

						queueTrackCount--;
					}
				}
				else {
					break;
				}
			} 
		}

		private DbTrack SelectTrackAtRandom( List<DbTrack> trackList ) {
			var r = new Random( DateTime.Now.Millisecond );
			var next = r.Next( trackList.Count );

			return( trackList.Skip( next ).FirstOrDefault());
		} 

		private List<DbTrack> BuildTrackList( IEnumerable<DbAlbum> albumList ) {
			var retValue = new List<DbTrack>();
			var minimumTrackDuration = new TimeSpan( 0, 0, 30 );

			foreach( var album in albumList ) {
				using( var albumTrackList = mTrackProvider.GetTrackList( album )) {
					retValue.AddRange( albumTrackList.List.Where( track => track.Duration > minimumTrackDuration ) );
				}
			}

			return( retValue );
		}

		private IEnumerable<DbAlbum> BuildAlbumList( long forArtist ) {
			var retValue = new List<DbAlbum>();

			using( var albumList = mAlbumProvider.GetAlbumList( forArtist )) {
				retValue.AddRange( albumList.List );
			}

			return ( retValue );
		} 
	}
}
