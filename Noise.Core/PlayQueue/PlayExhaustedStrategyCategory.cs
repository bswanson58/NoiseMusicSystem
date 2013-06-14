using System;
using System.Linq;
using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyCategory : IPlayExhaustedStrategy {
		private readonly IAlbumProvider	mAlbumProvider;
		private readonly ITrackProvider	mTrackProvider;
		private	readonly List<long>		mAlbums;
		private IPlayQueue				mQueueMgr;
		private long					mCategoryId;

		public PlayExhaustedStrategyCategory( IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mAlbums = new List<long>();
		}

		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.PlayCategory ); }
		}

		public bool QueueTracks( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			Condition.Requires( queueMgr ).IsNotNull();

			var retValue = false;

			mQueueMgr = queueMgr;

			if( parameters is PlayStrategyParameterDbId ) {
				var dbParam = parameters as PlayStrategyParameterDbId;

				if( mCategoryId != dbParam.DbItemId ) {
					mAlbums.Clear();
					mCategoryId = dbParam.DbItemId;
				}

				if( queueMgr != null ) {
					if(!mAlbums.Any()) {
						using( var albumList = mAlbumProvider.GetAlbumsInCategory( mCategoryId )) {
							mAlbums.AddRange( from album in albumList.List select album );
						}
					}

					retValue = QueueTracks( 3 - mQueueMgr.UnplayedTrackCount );
				}
			}

			return( retValue );
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
