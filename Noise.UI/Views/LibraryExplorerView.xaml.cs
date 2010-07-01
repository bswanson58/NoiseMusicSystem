using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for LibraryExplorerView.xaml
	/// </summary>
	public partial class LibraryExplorerView : UserControl {
		public LibraryExplorerView() {
			InitializeComponent();
/*
			var template = FindResource( "ArtistTemplate" ) as HierarchicalDataTemplate;
			if( template != null ) {
				template.ItemTemplate = FindResource( "AlbumTemplate" ) as HierarchicalDataTemplate;

				TreeView.ItemTemplate = template;
			}
*/		}
	}
}
