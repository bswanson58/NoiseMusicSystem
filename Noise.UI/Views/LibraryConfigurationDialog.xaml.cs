using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for LibraryConfigurationDialog.xaml
	/// </summary>
	[Export( DialogNames.LibraryConfiguration, typeof( FrameworkElement ))]
	public partial class LibraryConfigurationDialog {
		public LibraryConfigurationDialog() {
			InitializeComponent();
		}
	}
}
