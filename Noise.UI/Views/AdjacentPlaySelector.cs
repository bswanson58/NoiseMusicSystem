using System.Windows;
using System.Windows.Controls;
using Noise.Infrastructure.Dto;
using Noise.UI.Dto;

namespace Noise.UI.Views {
    public class AdjacentPlaySelector : DataTemplateSelector {
        public DataTemplate     PreviousTemplate { get; set; }
        public DataTemplate     NextTemplate { get; set; }
        public DataTemplate     BothTemplate { get; set; }
        public DataTemplate     NeitherTemplate { get; set; }
        public DataTemplate     DoNotPlayTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            var retValue = NeitherTemplate;

            if( item is CombinedPlayStrategy strategy ) {
                if( strategy.DoNotPlay ) {
                    retValue = DoNotPlayTemplate;
                }
                else {
                    switch( strategy.PlayStrategy ) {
                        case ePlayAdjacentStrategy.None:
                            retValue = NeitherTemplate;
                            break;

                        case ePlayAdjacentStrategy.PlayNext:
                            retValue = NextTemplate;
                            break;

                        case ePlayAdjacentStrategy.PlayNextPrevious:
                            retValue = BothTemplate;
                            break;

                        case ePlayAdjacentStrategy.PlayPrevious:
                            retValue = PreviousTemplate;
                            break;
                    }
                }
            }

            return retValue;
        }
    }
}
