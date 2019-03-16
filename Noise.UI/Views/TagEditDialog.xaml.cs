using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for TagEditDialog.xaml
    /// </summary>
    [Export(DialogNames.TagAddDialog, typeof( FrameworkElement ))]
    public partial class TagEditDialog {
        public TagEditDialog() {
            InitializeComponent();
        }
    }
}
