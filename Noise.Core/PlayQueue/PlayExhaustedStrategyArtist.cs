using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
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

		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.PlayArtist ); }
		}

		public bool QueueTracks( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			Condition.Requires( queueMgr ).IsNotNull();

			var retValue = false;

			mQueueMgr = queueMgr;

			if( parameters is PlayStrategyParameterDbId ) {
				var	dbParam = parameters as PlayStrategyParameterDbId;

				if( mArtistId != dbParam.DbItemId ) {
					mAlbums.Clear();
					mArtistId = dbParam.DbItemId;
				}

				if( queueMgr != null ) {
					if( !mAlbums.Any() ) {
						using( var albumList = mAlbumProvider.GetAlbumList( mArtistId ) ) {
							mAlbums.AddRange( from album in albumList.List select album.DbId );
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
				  ( mAlbums.Any()) &&
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
