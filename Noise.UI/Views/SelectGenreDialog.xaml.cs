using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for SelectGenreDialog.xaml
	/// </summary>
	[Export( DialogNames.SelectGenre, typeof( FrameworkElement ))]
	public partial class SelectGenreDialog {
		public SelectGenreDialog() {
			InitializeComponent();
		}
	}
}
