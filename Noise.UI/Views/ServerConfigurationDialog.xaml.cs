using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for ServerConfigurationDialog.xaml
	/// </summary>
	[Export( DialogNames.ServerConfiguration, typeof( FrameworkElement ))]
	public partial class ServerConfigurationDialog {
		public ServerConfigurationDialog() {
			InitializeComponent();
		}
	}
}
