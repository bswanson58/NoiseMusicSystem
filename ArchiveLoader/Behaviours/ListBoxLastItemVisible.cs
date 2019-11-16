using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArchiveLoader.Behaviours {
    public class ListBoxLastItemVisible {
        public static object GetChangeTrigger( ListBox listView ) {
            return( listView.GetValue( ChangeTriggerProperty ));
        }

        public static void SetChangeTrigger( ListView listView, object value ) {
            listView.SetValue( ChangeTriggerProperty, value );
        }

        public static readonly DependencyProperty ChangeTriggerProperty =
            DependencyProperty.RegisterAttached(
                "ChangeTrigger",
                typeof( object ),
                typeof( ListBoxLastItemVisible ),
                new UIPropertyMetadata( null, OnChangeTrigger ));

        static void OnChangeTrigger( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
            if(( depObj is ListBox listView ) &&
               ( args.NewValue != null )) {
                if( VisualTreeHelper.GetChild( listView, 0 ) is Border border ) {
                    var scrollViewer = VisualTreeHelper.GetChild( border, 0 ) as ScrollViewer;

                    scrollViewer?.ScrollToBottom();
                }
            }
        }
    }
}
