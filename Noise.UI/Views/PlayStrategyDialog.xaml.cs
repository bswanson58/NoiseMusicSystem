using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for PlayStrategyDialog.xaml
	/// </summary>
	[Export( DialogNames.PlayStrategyConfiguration, typeof( FrameworkElement ))]
	public partial class PlayStrategyDialog {
		public PlayStrategyDialog() {
			InitializeComponent();
		}
	}
}
