using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for LibraryBackupDialog.xaml
    /// </summary>
    [Export( DialogNames.LibraryBackup, typeof( FrameworkElement ))]
    public partial class LibraryBackupDialog {
        public LibraryBackupDialog() {
            InitializeComponent();
        }
    }
}
