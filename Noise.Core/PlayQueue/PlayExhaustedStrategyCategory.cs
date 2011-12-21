using System;
using System.Linq;
using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyCategory : IPlayExhaustedStrategy {
		private readonly IDataProvider	mDataProvider;
		private	readonly List<long>		mAlbums;
		private IPlayQueue				mQueueMgr;
		private long					mCategoryId;

		public PlayExhaustedStrategyCategory( IDataProvider dataProvider ) {
			mDataProvider = dataProvider;
			mAlbums = new List<long>();
		}

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			mQueueMgr = queueMgr;
			mCategoryId = itemId;

			using( var albumList = mDataProvider.GetAlbumsInCategory( mCategoryId )) {
				mAlbums.Clear();
				mAlbums.AddRange( albumList.List );
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
			
				using( var trackList = mDataProvider.GetTrackList( albumId )) {
					next = r.Next( trackList.List.Count());

					var track = trackList.List.Skip( next ).FirstOrDefault();
					if((!mQueueMgr.IsTrackQueued( track )) &&
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
