using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Noise.UI.Dto;

namespace Noise.UI.ViewModels {
	public interface ILibraryExplorerViewModel {
		void	SetViewTemplate( DataTemplate template );
		void	SetSearchOptions( IEnumerable<string> searchOptions );
		void	SetTreeSortDescription( IEnumerable<SortDescription> descriptions );

		Collection<UiTreeNode> TreeData { get; }
	}
}
