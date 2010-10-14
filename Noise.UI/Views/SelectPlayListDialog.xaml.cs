using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for SelectPlayListDialog.xaml
	/// </summary>
	[Export( DialogNames.SelectPlayList, typeof( FrameworkElement ))]
	public partial class SelectPlayListDialog {
		public SelectPlayListDialog() {
			InitializeComponent();
		}
	}
}
