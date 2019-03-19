using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for TagAddDialog.xaml
    /// </summary>
    [Export(DialogNames.TagAddDialog, typeof( FrameworkElement ))]
    public partial class TagAddDialog {
        public TagAddDialog() {
            InitializeComponent();
        }
    }
}
