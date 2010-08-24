using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for DatabaseConfigurationDialog.xaml
	/// </summary>
	[Export( DialogNames.DatabaseConfiguration, typeof( FrameworkElement ))]
	public partial class DatabaseConfigurationDialog {
		public DatabaseConfigurationDialog() {
			InitializeComponent();
		}
	}
}
