using System.Windows;
using System.Windows.Controls;
using Noise.Desktop.ViewModels;

namespace Noise.Desktop.Views {
    class ShellViewSelector : DataTemplateSelector {
        public  DataTemplate    StartupView { get; set; }
        public  DataTemplate    LibraryCreationView { get ;set; }
        public  DataTemplate    LibrarySelectionView { get; set; }
        public  DataTemplate    LibraryView { get; set; }
        public  DataTemplate    ListeningView {  get; set; }
        public  DataTemplate    TimelineView {  get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            var retValue = StartupView;

            if( item is ShellViews viewEnum ) {
                switch( viewEnum ) {
                    case ShellViews.Startup:
                        retValue = StartupView;
                        break;

                    case ShellViews.LibraryCreation:
                        retValue = LibrarySelectionView;
                        break;

                    case ShellViews.LibrarySelection:
                        retValue = LibraryCreationView;
                        break;

                    case ShellViews.Library:
                        retValue = LibraryView;
                        break;

                    case ShellViews.Listening:
                        retValue = ListeningView;
                        break;

                    case ShellViews.Timeline:
                        retValue = TimelineView;
                        break;
                }
            }

            return retValue;
        }
    }
}
