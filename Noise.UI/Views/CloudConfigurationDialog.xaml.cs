using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for CloudConfigurationDialog.xaml
	/// </summary>
	[Export( DialogNames.CloudConfiguration, typeof( FrameworkElement ))]
	public partial class CloudConfigurationDialog {
		public CloudConfigurationDialog() {
			InitializeComponent();
		}
	}
}
