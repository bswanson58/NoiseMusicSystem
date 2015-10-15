using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for PlaybackContextDialog.xaml
	/// </summary>
	[Export( DialogNames.ManagePlaybackContext, typeof( FrameworkElement ))]
	public partial class PlaybackContextDialog {
		public PlaybackContextDialog() {
			InitializeComponent();
		}
	}
}
