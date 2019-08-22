using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class BasePlayStrategyParameters : IPlayStrategyParameters {
		public eTrackPlayHandlers   ForPlayStrategy { get; set; }
		public string				ParameterType { get; set; }

		public BasePlayStrategyParameters( eTrackPlayHandlers strategy, string parameterType ) {
			ForPlayStrategy = strategy;
			ParameterType = parameterType;
		}
	}
}
