using System.Windows;
using System.Windows.Controls;
using Noise.UI.ViewModels;

namespace Noise.UI.Views {
    class PlayerTemplateSelector : DataTemplateSelector {
        public  DataTemplate    StandardPlayer { get; set; }
        public  DataTemplate    ExtendedPlayer { get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            var retValue = StandardPlayer;

            if( item is PlayerViews viewEnum ) {
                switch( viewEnum ) {
                    case  PlayerViews.Regular:
                        retValue = StandardPlayer;
                        break;

                    case PlayerViews.Extended:
                        retValue = ExtendedPlayer;
                        break;
                }
            }

            return retValue;
        }
    }
}
