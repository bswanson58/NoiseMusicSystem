using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for LyricsEditDialog.xaml
	/// </summary>
	[Export( DialogNames.LyricsEdit, typeof( FrameworkElement ))]
	public partial class LyricsEditDialog {
		public LyricsEditDialog() {
			InitializeComponent();
		}
	}
}
