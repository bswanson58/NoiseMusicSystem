using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	interface IPlayExhaustedStrategy {
		ePlayExhaustedStrategy	PlayStrategy { get; }

		bool					QueueExhausted( IPlayQueue queueMgr, long itemId );
		void					NextTrackPlayed();
	}
}
