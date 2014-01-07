using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Dto {
	public class PlayStrategyItem {
		public string			Title { get; private set; }
		public ePlayStrategy    StrategyId { get; private set; }

		public PlayStrategyItem( ePlayStrategy strategyId, string title ) {
			StrategyId = strategyId;
			Title = title;
		}
	}
}
