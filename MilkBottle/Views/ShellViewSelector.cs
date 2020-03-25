using System.Windows;
using System.Windows.Controls;
using MilkBottle.ViewModels;

namespace MilkBottle.Views {
    class ShellViewSelector : DataTemplateSelector {
        public  DataTemplate    ManualView { get; set; }
        public  DataTemplate    ReviewView { get ;set; }
        public  DataTemplate    SyncView { get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            var retValue = ManualView;

            if( item is ShellView viewEnum ) {
                switch( viewEnum ) {
                    case ShellView.Manual:
                        retValue = ManualView;
                        break;

                    case ShellView.Review:
                        retValue = ReviewView;
                        break;

                    case ShellView.Sync:
                        retValue = SyncView;
                        break;
                }
            }

            return retValue;
        }
    }
}
