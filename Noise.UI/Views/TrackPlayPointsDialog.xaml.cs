using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for TrackFadePointsDialog.xaml
    /// </summary>
    [Export( DialogNames.TrackPlayPointsDialog, typeof( FrameworkElement ))]
    public partial class TrackPlayPointsDialog {
        public TrackPlayPointsDialog() {
            InitializeComponent();
        }
    }
}
