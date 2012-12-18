using CuttingEdge.Conditions;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyReplay : IPlayExhaustedStrategy {
		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.Replay ); }
		}

		public bool QueueTracks( IPlayQueue queueMgr, long itemId ) {
			Condition.Requires( queueMgr ).IsNotNull();

			var retValue = false;

			if( queueMgr.UnplayedTrackCount == 0 ) {
				foreach( var track in queueMgr.PlayList ) {
					if(!track.IsPlaying ) {
						track.HasPlayed = false;

						retValue = true;
					}
				}
			}

			return( retValue );
		}
	}
}
