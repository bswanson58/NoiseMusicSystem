using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for TrackStrategyOptionsDialog.xaml
    /// </summary>
    [Export( DialogNames.TrackStrategyOptionsDialog, typeof( FrameworkElement ))]
    public partial class TrackStrategyOptionsDialog {
        public TrackStrategyOptionsDialog() {
            InitializeComponent();
        }
    }
}
