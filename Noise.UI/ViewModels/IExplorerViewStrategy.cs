using System.Collections.ObjectModel;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	internal interface IExplorerViewStrategy {
		void	Initialize( LibraryExplorerViewModel viewModel );
		void	PopulateTree( ObservableCollection<ExplorerTreeNode> tree );

		bool	Search( string searchText );
		void	ClearCurrentSearch();
	}
}
