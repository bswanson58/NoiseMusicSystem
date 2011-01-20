using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for AlbumEditDialog.xaml
	/// </summary>
	[Export( DialogNames.AlbumEdit, typeof( FrameworkElement ))]
	public partial class AlbumEditDialog {
		public AlbumEditDialog() {
			InitializeComponent();
		}
	}
}
