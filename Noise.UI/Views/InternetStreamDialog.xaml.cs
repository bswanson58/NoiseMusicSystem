using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for InternetStreamDialog.xaml
	/// </summary>
	[Export( DialogNames.InternetStreamEdit, typeof( FrameworkElement ))]
	public partial class InternetStreamDialog {
		public InternetStreamDialog() {
			InitializeComponent();
		}
	}
}
