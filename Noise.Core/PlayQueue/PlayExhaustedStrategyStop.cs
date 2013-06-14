using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyStop : IPlayExhaustedStrategy {
		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.Stop ); }
		}

		public bool QueueTracks( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			return( false );
		}
	}
}
