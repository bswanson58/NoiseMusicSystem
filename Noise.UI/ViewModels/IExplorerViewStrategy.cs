using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Dto;

namespace Noise.UI.ViewModels {
	public interface IExplorerViewStrategy {
		string					StrategyId { get; }
		string					StrategyName { get; }
		bool					IsDefaultStrategy{ get; }

		void					Initialize( LibraryExplorerViewModel viewModel );
		void					UseSortPrefixes( bool enable, IEnumerable<string> sortPrefixes );

		void					Activate();
		void					Deactivate();

		IEnumerable<UiTreeNode>	BuildTree( IDatabaseFilter filter );
		IEnumerable<IndexNode>	BuildIndex( IEnumerable<UiTreeNode> artistList );

		bool					Search( string searchText, IEnumerable<string> searchOptions );
		void					ClearCurrentSearch();

		void					ConfigureView();
	}
}
