using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Dto {
	public class ExhaustedStrategyItem {
		public string					Title { get; private set; }
		public ePlayExhaustedStrategy	Strategy { get; private set; }

		public ExhaustedStrategyItem( ePlayExhaustedStrategy strategy, string title ) {
			Strategy = strategy;
			Title = title;
		}
	}
}
