using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for TagAssociationDialog.xaml
    /// </summary>
    [Export(DialogNames.TagAssociationDialog, typeof( FrameworkElement ))]
    public partial class TagAssociationDialog {
        public TagAssociationDialog() {
            InitializeComponent();
        }
    }
}
