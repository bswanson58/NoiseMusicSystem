using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for CategoryEditDialog.xaml
	/// </summary>
	[Export( DialogNames.CategoryEdit, typeof( FrameworkElement ))]
	public partial class CategoryEditDialog {
		public CategoryEditDialog() {
			InitializeComponent();
		}
	}
}
