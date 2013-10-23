using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyCategory : PlayExhaustedStrategyBase {
		private readonly IAlbumProvider	mAlbumProvider;
		private readonly ITrackProvider	mTrackProvider;
		private readonly ITagProvider	mTagProvider;
		private	readonly List<long>		mAlbums;
		private long					mCategoryId;
		private string					mCategoryName;

		public PlayExhaustedStrategyCategory( IAlbumProvider albumProvider, ITrackProvider trackProvider, ITagProvider tagProvider ) :
			base( ePlayExhaustedStrategy.PlayCategory, "Play Category...", true ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mTagProvider = tagProvider;

			mAlbums = new List<long>();
		}

		protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
			if( parameters is PlayStrategyParameterDbId ) {
				var dbParam = parameters as PlayStrategyParameterDbId;

				if( mCategoryId != dbParam.DbItemId ) {
					mAlbums.Clear();
					mCategoryId = dbParam.DbItemId;

					var tag = ( from t in mTagProvider.GetTagList( eTagGroup.User ).List where t.DbId == mCategoryId select t ).FirstOrDefault();
					if( tag != null ) {
						mCategoryName = tag.Name;
					}
				}
			}
		}

		protected override string FormatDescription() {
			return( string.Format( "play tracks from category {0}", mCategoryName ));
		}

		protected override DbTrack SelectATrack() {
			return( null );
		}

		public override bool QueueTracks() {
			Condition.Requires( mQueueMgr ).IsNotNull();

			if(!mAlbums.Any()) {
				using( var albumList = mAlbumProvider.GetAlbumsInCategory( mCategoryId )) {
					mAlbums.AddRange( from album in albumList.List select album );
				}
			}

			return( QueueTracks( 3 - mQueueMgr.UnplayedTrackCount ));
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
