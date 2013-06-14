using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class BasePlayStrategyParameters : IPlayStrategyParameters {
		public ePlayExhaustedStrategy	ForPlayStrategy { get; set; }
		public string					ParameterType { get; set; }

		public BasePlayStrategyParameters( ePlayExhaustedStrategy strategy, string parameterType ) {
			ForPlayStrategy = strategy;
			ParameterType = parameterType;
		}
	}
}
