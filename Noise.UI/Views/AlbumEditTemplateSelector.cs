using System.Windows;
using System.Windows.Controls;
using Noise.UI.ViewModels;

namespace Noise.UI.Views {
    public class AlbumEditTemplateSelector : DataTemplateSelector {
        public DataTemplate     AlbumTemplate { get; set; }
        public DataTemplate     VolumeTemplate { get; set; }
        public DataTemplate     TrackTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            var retValue = default( DataTemplate );

            if( item is UiAlbumEdit ) {
                retValue = AlbumTemplate;
            }
            else if( item is UiVolumeEdit ) {
                retValue = VolumeTemplate;
            }
            else if( item is UiTrackEdit ) {
                retValue = TrackTemplate;
            }

            return retValue;
        }
    }
}
