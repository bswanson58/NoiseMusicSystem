using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyStop : IPlayExhaustedStrategy {
		public bool QueueExhausted( IPlayQueue queueMgr ) {
			return( false );
		}

		public void NextTrackPlayed() {
		}
	}
}
