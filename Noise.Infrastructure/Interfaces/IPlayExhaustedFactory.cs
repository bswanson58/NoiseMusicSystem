using System.Collections.Generic;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayExhaustedFactory {
	    IEnumerable<IPlayExhaustedStrategy>	AvailableStrategies { get; }

		IPlayExhaustedStrategy				ProvideExhaustedStrategy( ePlayExhaustedStrategy strategy );
	}
}
