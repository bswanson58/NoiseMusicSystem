using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for TrackEditDialog.xaml
	/// </summary>
	[Export( DialogNames.TrackEdit, typeof( FrameworkElement ))]
	public partial class TrackEditDialog {
		public TrackEditDialog() {
			InitializeComponent();
		}
	}
}
