using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyPlayList : IPlayExhaustedStrategy {
		private readonly	IPlayListProvider	mPlayListProvider;
		private	readonly	ITrackProvider		mTrackProvider;

		public PlayExhaustedStrategyPlayList( IPlayListProvider playListProvider, ITrackProvider trackProvider ) {
			mPlayListProvider = playListProvider;
			mTrackProvider = trackProvider;
		}

		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.PlayList ); }
		}

		public bool QueueTracks( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			Condition.Requires( queueMgr ).IsNotNull();

			var retValue = false;

			if( parameters is PlayStrategyParameterDbId ) {
				var dbParam = parameters as PlayStrategyParameterDbId;

				var	playList = mPlayListProvider.GetPlayList( dbParam.DbItemId );

				if( playList != null ) {
					var tracks = mTrackProvider.GetTrackListForPlayList( playList );

					foreach( var track in tracks ) {
						if(!queueMgr.IsTrackQueued( track )) {
							queueMgr.Add( track );

							retValue = true;
						}
					}
				}
			}

			return( retValue );
		}
	}
}
