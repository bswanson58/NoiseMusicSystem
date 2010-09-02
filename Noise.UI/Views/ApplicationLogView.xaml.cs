using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for ApplicationLogView.xaml
	/// </summary>
	[Export( DialogNames.ApplicationLogView, typeof( FrameworkElement ))]
	public partial class ApplicationLogView {
		public ApplicationLogView() {
			InitializeComponent();
		}
	}
}
