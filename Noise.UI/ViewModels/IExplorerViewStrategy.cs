using System.Collections.Generic;
using System.Collections.ObjectModel;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	internal interface IExplorerViewStrategy {
		void	Initialize( LibraryExplorerViewModel viewModel );
		IEnumerable<ExplorerTreeNode>	BuildTree( IDatabaseFilter filter );

		bool	Search( string searchText, IEnumerable<string> searchOptions );
		void	ClearCurrentSearch();
	}
}
