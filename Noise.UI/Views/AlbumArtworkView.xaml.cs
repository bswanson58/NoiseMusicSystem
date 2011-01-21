using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for AlbumArtworkView.xaml
	/// </summary>
	[Export( DialogNames.AlbumArtworkDisplay, typeof( FrameworkElement ))]
	public partial class AlbumArtworkView {
		public AlbumArtworkView() {
			InitializeComponent();
		}
	}
}
