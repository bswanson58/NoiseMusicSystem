using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	interface IPlayExhaustedStrategy {
		ePlayExhaustedStrategy	PlayStrategy { get; }

		bool					QueueTracks( IPlayQueue queueMgr, long itemId );
	}
}
