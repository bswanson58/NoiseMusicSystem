using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for PlayListDialog.xaml
	/// </summary>
	[Export( DialogNames.PlayListEdit, typeof( FrameworkElement ))]
	public partial class PlayListDialog {
		public PlayListDialog() {
			InitializeComponent();
		}
	}
}
