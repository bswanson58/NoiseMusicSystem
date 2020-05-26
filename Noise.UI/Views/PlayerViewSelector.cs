using System.Windows;
using System.Windows.Controls;
using Noise.UI.ViewModels;

namespace Noise.UI.Views {
    class PlayerViewSelector : DataTemplateSelector {
        public  DataTemplate    NormalPlayerView {  get; set; }
        public  DataTemplate    ExtendedPlayerView {  get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            var retValue = NormalPlayerView;

            if( item is PlayerViews viewEnum ) {
                switch( viewEnum ) {
                    case PlayerViews.Regular:
                        retValue = NormalPlayerView;
                        break;

                    case PlayerViews.Extended:
                        retValue = ExtendedPlayerView;
                        break;
                }
            }

            return retValue;
        }
    }
}
