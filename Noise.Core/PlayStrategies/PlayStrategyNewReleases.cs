using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public class PlayStrategyNewReleases : IPlayStrategy {
		private readonly ITrackProvider		mTrackProvider;
		private readonly List<DbTrack>		mTracks;
        private IPlayQueue                  mPlayQueue;
		private int							mNextFeaturedPlay;
		private readonly Random				mRandom;
 
		public PlayStrategyNewReleases( ITrackProvider trackProvider ) {
			mTrackProvider = trackProvider;

			mTracks = new List<DbTrack>();
			mRandom = new Random( DateTime.Now.Millisecond );

			mNextFeaturedPlay = NextPlayInterval();
		}

	    public ePlayStrategy StrategyId {
            get {  return( ePlayStrategy.NewReleases ); }
	    }
	    public string DisplayName {
            get {  return( "New Releases" ); }
	    }

	    public string StrategyDescription {
            get {  return( "with occasional new library tracks" ); }
	    }

	    public bool RequiresParameters {
            get {  return( false ); }
	    }

	    public bool Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
            mPlayQueue = queueMgr;

            return( true );
	    }

		public PlayQueueTrack NextTrack() {
			var retValue = mPlayQueue.PlayList.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed ));

			mNextFeaturedPlay--;

			if( mNextFeaturedPlay < 0 ) {
				mNextFeaturedPlay = NextPlayInterval();

                if( !mTracks.Any()) {
					LoadNewReleases();
				}

				if( mTracks.Any()) {
					var trackIndex = mRandom.Next( mTracks.Count - 1 );

					mPlayQueue.StrategyAdd( mTracks[trackIndex], retValue );
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
