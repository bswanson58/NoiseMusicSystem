using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyReplay : PlayExhaustedStrategyBase {
		public PlayExhaustedStrategyReplay() :
			base( ePlayExhaustedStrategy.Replay, "Replay", "Replay the tracks in the play queue." ) {
		}

		protected override string FormatDescription() {
			return( string.Format( "replay the tracks" ));
		}

		protected override DbTrack SelectATrack() {
			throw new System.NotImplementedException();
		}

		public override bool QueueTracks() {
			Condition.Requires( PlayQueueMgr ).IsNotNull();

			var retValue = false;

			if( PlayQueueMgr.UnplayedTrackCount == 0 ) {
				foreach( var track in PlayQueueMgr.PlayList ) {
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
