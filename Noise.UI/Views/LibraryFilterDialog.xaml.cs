using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for LibraryFilterDialog.xaml
	/// </summary>
	[Export( DialogNames.LibraryExplorerFilter, typeof( FrameworkElement ))]
	public partial class LibraryFilterDialog {
		public LibraryFilterDialog() {
			InitializeComponent();
		}
	}
}
