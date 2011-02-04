using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Dto;

namespace Noise.UI.ViewModels {
	internal interface IExplorerViewStrategy {
		void					Initialize( LibraryExplorerViewModel viewModel );
		void					UseSortPrefixes( bool enable, IEnumerable<string> sortPrefixes );

		IEnumerable<UiTreeNode>	BuildTree( IDatabaseFilter filter );
		IEnumerable<IndexNode>	BuildIndex( IEnumerable<UiTreeNode> artistList );

		bool					Search( string searchText, IEnumerable<string> searchOptions );
		void					ClearCurrentSearch();
	}
}
