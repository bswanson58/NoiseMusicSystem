using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for TrackFadePointsDialog.xaml
    /// </summary>
    [Export( DialogNames.TrackFadePointsDialog, typeof( FrameworkElement ))]
    public partial class TrackFadePointsDialog {
        public TrackFadePointsDialog() {
            InitializeComponent();
        }
    }
}
