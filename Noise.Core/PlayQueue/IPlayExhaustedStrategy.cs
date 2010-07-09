namespace Noise.Core.PlayQueue {
	interface IPlayExhaustedStrategy {
		bool	QueueExhausted( PlayQueueMgr queueMgr );
	}
}
