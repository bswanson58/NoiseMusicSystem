using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal interface IPlayStrategyFactory {
		IPlayStrategy	ProvidePlayStrategy( ePlayStrategy strategy );
	}
}
