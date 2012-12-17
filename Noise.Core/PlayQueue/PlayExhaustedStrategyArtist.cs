﻿using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyArtist : IPlayExhaustedStrategy {
		private readonly IAlbumProvider	mAlbumProvider;
		private readonly ITrackProvider	mTrackProvider;
		private	readonly List<long>		mAlbums;
		private IPlayQueue				mQueueMgr;
		private long					mArtistId;

		public PlayExhaustedStrategyArtist( IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mAlbums = new List<long>();
		}

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			mQueueMgr = queueMgr;
			mArtistId = itemId;

			using( var albumList = mAlbumProvider.GetAlbumList( mArtistId )) {
				mAlbums.Clear();
				mAlbums.AddRange( from album in albumList.List select album.DbId );
			}

			return( QueueTracks( 3 ));
		}

		public void NextTrackPlayed() {
			if( ( mQueueMgr != null ) &&
			   ( mQueueMgr.StrategyRequestsQueued ) ) {
				var	trackCount = mQueueMgr.UnplayedTrackCount;

				if( trackCount < 3 ) {
					QueueTracks( 3 - trackCount );
				}
			}
		}

		private bool QueueTracks( int trackCount ) {
			var retValue = false;
			var r = new Random( DateTime.Now.Millisecond );
			var circuitBreaker = 100;

			while(( trackCount > 0 ) &&
			      ( circuitBreaker > 0 )) {
				var next = r.Next( mAlbums.Count );
				var albumId = mAlbums.Skip( next ).FirstOrDefault();
			
				using( var trackList = mTrackProvider.GetTrackList( albumId )) {
					next = r.Next( trackList.List.Count());

					var track = trackList.List.Skip( next ).FirstOrDefault();
					if((!mQueueMgr.IsTrackQueued( track )) &&
					   ( track != null ) &&
					   ( track.Rating >= 0 )) {
						mQueueMgr.StrategyAdd( track );

						retValue = true;
						trackCount--;
					}
				}

				circuitBreaker--;
			}

			return( retValue );
		}
	}
}
