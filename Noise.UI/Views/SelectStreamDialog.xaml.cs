using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for SelectStreamDialog.xaml
	/// </summary>
	[Export( DialogNames.SelectStream, typeof( FrameworkElement ))]
	public partial class SelectStreamDialog {
		public SelectStreamDialog() {
			InitializeComponent();
		}
	}
}
