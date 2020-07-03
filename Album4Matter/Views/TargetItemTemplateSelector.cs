using System.Windows;
using System.Windows.Controls;
using Album4Matter.Dto;

namespace Album4Matter.Views {
    class TargetItemTemplateSelector : DataTemplateSelector {
        public  DataTemplate    FileTemplate { get; set; }
        public  DataTemplate    FolderTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item, DependencyObject container ) {
            DataTemplate    retValue = null;

            if( item is TargetFile ) {
                retValue = FileTemplate;
            }
            else if( item is TargetFolder ) {
                retValue = FolderTemplate;
            }

            return retValue;
        }
    }
}
