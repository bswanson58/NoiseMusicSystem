using System.Collections.Generic;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayStrategyFactory {
        IEnumerable<IPlayStrategy> AvailableStrategies {  get; }

		IPlayStrategy	ProvidePlayStrategy( ePlayStrategy strategyId );
	}
}
