using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	internal interface IExplorerViewStrategy {
		void	Initialize( LibraryExplorerViewModel viewModel );

		IEnumerable<ArtistTreeNode>	BuildTree( IDatabaseFilter filter );
		IEnumerable<IndexNode> BuildIndex( IEnumerable<ArtistTreeNode> artistList );

		bool	Search( string searchText, IEnumerable<string> searchOptions );
		void	ClearCurrentSearch();
	}
}
