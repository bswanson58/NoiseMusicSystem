using System.Windows;
using System.Windows.Controls;
using TuneRenamer.Dto;

namespace TuneRenamer.Views {
    class SourceItemTemplateSelector : DataTemplateSelector {
        public  DataTemplate    FileTemplate { get; set; }
        public  DataTemplate    FolderTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            DataTemplate    retValue = null;

            if( item is SourceFile ) {
                retValue = FileTemplate;
            }
            else if( item is SourceFolder ) {
                retValue = FolderTemplate;
            }

            return retValue;
        }
    }
}
