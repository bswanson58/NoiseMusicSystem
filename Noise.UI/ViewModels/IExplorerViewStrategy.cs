﻿using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	internal interface IExplorerViewStrategy {
		void	Initialize( LibraryExplorerViewModel viewModel );
		void	UseSortPrefixes( bool enable, IEnumerable<string> sortPrefixes );

		IEnumerable<object>		BuildTree( IDatabaseFilter filter );
		IEnumerable<IndexNode>	BuildIndex( IEnumerable<object> artistList );

		bool	Search( string searchText, IEnumerable<string> searchOptions );
		void	ClearCurrentSearch();
	}
}
