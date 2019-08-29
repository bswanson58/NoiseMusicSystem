using System.ComponentModel.Composition;
using System.Windows;
using Noise.UI.Support;

namespace Noise.UI.Views {
    /// <summary>
    /// Interaction logic for ArtistArtworkView.xaml
    /// </summary>
    [Export( DialogNames.ArtistArtworkDisplay, typeof( FrameworkElement ))]
    public partial class ArtistArtworkView {
        public ArtistArtworkView() {
            InitializeComponent();
        }
    }
}
