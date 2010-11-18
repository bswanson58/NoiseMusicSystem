using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for EqDialog.xaml
	/// </summary>
	[Export( DialogNames.EqDialog, typeof( FrameworkElement ))]
	public partial class EqDialog {
		public EqDialog() {
			InitializeComponent();
		}
	}
}
