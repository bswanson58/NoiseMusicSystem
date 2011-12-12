using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal interface IPlayExhaustedFactory {
		IPlayExhaustedStrategy	ProvideExhaustedStrategy( ePlayExhaustedStrategy strategy );
	}
}
