using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for SelectArtistDialog.xaml
	/// </summary>
	[Export( DialogNames.SelectArtist, typeof( FrameworkElement ))]
	public partial class SelectArtistDialog {
		public SelectArtistDialog() {
			InitializeComponent();
		}
	}
}
