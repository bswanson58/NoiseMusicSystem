using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Dto {
	public class PlayStrategyItem {
		public string			Title { get; private set; }
		public ePlayStrategy	Strategy { get; private set; }

		public PlayStrategyItem( ePlayStrategy strategy, string title ) {
			Strategy = strategy;
			Title = title;
		}
	}
}
