using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for ArtistEditDialog.xaml
	/// </summary>
	[Export( DialogNames.ArtistEdit, typeof( FrameworkElement ))]
	public partial class ArtistEditDialog {
		public ArtistEditDialog() {
			InitializeComponent();
		}
	}
}
