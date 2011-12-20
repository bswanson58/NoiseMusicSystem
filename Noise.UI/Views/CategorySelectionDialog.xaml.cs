using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for CategorySelectionDialog.xaml
	/// </summary>
	[Export( DialogNames.CategorySelection, typeof( FrameworkElement ))]
	public partial class CategorySelectionDialog {
		public CategorySelectionDialog() {
			InitializeComponent();
		}
	}
}
