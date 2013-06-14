using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for FeaturedArtistsDialog.xaml
	/// </summary>
	[Export( DialogNames.FeaturedArtistsSelect, typeof( FrameworkElement ) )]
	public partial class FeaturedArtistsDialog {
		public FeaturedArtistsDialog() {
			InitializeComponent();
		}
	}
}
