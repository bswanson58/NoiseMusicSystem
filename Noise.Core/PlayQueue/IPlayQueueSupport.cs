using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public interface IPlayQueueSupport {
		bool		Initialize( IPlayQueue playQueueMgr );
	}
}
