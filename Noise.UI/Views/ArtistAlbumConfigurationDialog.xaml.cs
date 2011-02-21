using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for ArtistAlbumConfigurationDialog.xaml
	/// </summary>
	[Export( DialogNames.ArtistAlbumConfiguration, typeof( FrameworkElement ))]
	public partial class ArtistAlbumConfigurationDialog {
		public ArtistAlbumConfigurationDialog() {
			InitializeComponent();
		}
	}
}
