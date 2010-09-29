using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for SearchViewDialog.xaml
	/// </summary>
	[Export( DialogNames.Search, typeof( FrameworkElement ))]
	public partial class SearchViewDialog {
		public SearchViewDialog() {
			InitializeComponent();
		}
	}
}
