using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for SelectCategoryDialog.xaml
	/// </summary>
	[Export( DialogNames.SelectCategory, typeof( FrameworkElement ))]
	public partial class SelectCategoryDialog {
		public SelectCategoryDialog() {
			InitializeComponent();
		}
	}
}
