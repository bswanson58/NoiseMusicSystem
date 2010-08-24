using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for ConfigurationDialog.xaml
	/// </summary>
	[Export( DialogNames.NoiseOptions, typeof( FrameworkElement ))]
	public partial class ConfigurationDialog {
		public ConfigurationDialog() {
			InitializeComponent();
		}
	}
}
