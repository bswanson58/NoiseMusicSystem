using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	interface IPlayExhaustedStrategy {
		bool	QueueExhausted( IPlayQueue queueMgr, long itemId );
		void	NextTrackPlayed();
	}
}
