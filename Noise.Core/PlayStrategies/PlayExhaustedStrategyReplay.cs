using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyReplay : PlayExhaustedStrategyBase {
		public PlayExhaustedStrategyReplay() :
			base( ePlayExhaustedStrategy.Replay, "Replay", false ) {
		}

		protected override string FormatDescription() {
			return( string.Format( "replay the tracks" ));
		}

		protected override DbTrack SelectATrack() {
			throw new System.NotImplementedException();
		}

		public override bool QueueTracks() {
			Condition.Requires( mQueueMgr ).IsNotNull();

			var retValue = false;

			if( mQueueMgr.UnplayedTrackCount == 0 ) {
				foreach( var track in mQueueMgr.PlayList ) {
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
