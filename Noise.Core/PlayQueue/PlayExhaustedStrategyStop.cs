using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyStop : IPlayExhaustedStrategy {
		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.Stop ); }
		}

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			return( false );
		}

		public void NextTrackPlayed() {
		}
	}
}
