namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyStop : IPlayExhaustedStrategy {
		public bool QueueExhausted( PlayQueueMgr queueMgr ) {
			return( false );
		}
	}
}
