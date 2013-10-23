using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyStop : PlayExhaustedStrategyBase {
		public PlayExhaustedStrategyStop() :
			base( ePlayExhaustedStrategy.Stop, "Stop Playing", false ) {
		}

		protected override string FormatDescription() {
			return( string.Format( "stop play" ));
		}

		protected override DbTrack SelectATrack() {
			throw new System.NotImplementedException();
		}

		public override bool QueueTracks() {
			return( false );
		}
	}
}
