using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public class PlayStrategyNewReleases : PlayStrategyBase {
		private readonly ITrackProvider		mTrackProvider;
		private readonly List<DbTrack>		mTracks;
		private int							mNextFeaturedPlay;
		private readonly Random				mRandom;
 
		public PlayStrategyNewReleases( ITrackProvider trackProvider ) :
			base( ePlayStrategy.NewReleases, "New Releases", "Occasionally adds a random track from albums that were recently added to the library." ) {
			mTrackProvider = trackProvider;

			mTracks = new List<DbTrack>();
			mRandom = new Random( DateTime.Now.Millisecond );

			mNextFeaturedPlay = NextPlayInterval();
		}

	    protected override string FormatDescription() {
            return( "with occasional new library tracks" );
	    }

		public override PlayQueueTrack NextTrack() {
			var retValue = PlayQueueMgr.PlayList.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed ));

			mNextFeaturedPlay--;

			if( mNextFeaturedPlay < 0 ) {
				mNextFeaturedPlay = NextPlayInterval();

                if( !mTracks.Any()) {
					LoadNewReleases();
				}

				if( mTracks.Any()) {
					var trackIndex = mRandom.Next( mTracks.Count );

					PlayQueueMgr.StrategyAdd( mTracks[trackIndex], retValue );
				}
			}


			return( retValue );
		}

		private void LoadNewReleases() {
			using( var trackList = mTrackProvider.GetNewlyAddedTracks()) {
				if( trackList.List != null ) {
					mTracks.AddRange( trackList.List.Where( track => track.Rating >= 0 ));
				}
			}
		}

		private int NextPlayInterval() {
			return ( mRandom.Next( 7 ) + 3 );
		}
	}
}
