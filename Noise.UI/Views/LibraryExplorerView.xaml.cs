using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for LibraryExplorerView.xaml
	/// </summary>
	public partial class LibraryExplorerView : UserControl {
		public LibraryExplorerView() {
			InitializeComponent();

			var template = FindResource( "ArtistTreeTemplate" ) as HierarchicalDataTemplate;
			if( template != null ) {
				template.ItemTemplate = FindResource( "AlbumTemplate" ) as DataTemplate;

				TreeView.ItemTemplate = template;
			}
		}
	}
}
