using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyArtist : PlayExhaustedStrategyBase {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private	readonly List<long>			mAlbums;
		private long						mArtistId;
		private string						mArtistName;

		public PlayExhaustedStrategyArtist( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
			base( ePlayExhaustedStrategy.PlayArtist, "Play Artist...", true ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mAlbums = new List<long>();
		}

		protected override string FormatDescription() {
			return( string.Format( "play tracks from {0}", mArtistName ));
		}

		protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
			if( mParameters is PlayStrategyParameterDbId ) {
				var	dbParam = mParameters as PlayStrategyParameterDbId;

				if( mArtistId != dbParam.DbItemId ) {
					mAlbums.Clear();
					mArtistId = dbParam.DbItemId;

					var artist = mArtistProvider.GetArtist( mArtistId );
					if( artist != null ) {
						mArtistName = artist.Name;
					}
				}
			}
		}

		protected override DbTrack SelectATrack() {
			throw new NotImplementedException();
		}

		public override bool QueueTracks() {
			Condition.Requires( mQueueMgr ).IsNotNull();

			var retValue = false;

			if( mQueueMgr != null ) {
				if(!mAlbums.Any()) {
					using( var albumList = mAlbumProvider.GetAlbumList( mArtistId )) {
						mAlbums.AddRange( from album in albumList.List select album.DbId );
					}
				}

				retValue = QueueTracks( 3 - mQueueMgr.UnplayedTrackCount );
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
