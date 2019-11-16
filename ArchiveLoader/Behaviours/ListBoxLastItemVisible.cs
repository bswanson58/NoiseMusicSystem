using System.Windows;
using System.Windows.Controls;

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
            if( depObj is ListBox list ) {
                var lastItem = list.Items.Count - 1;

                if( lastItem > 1 ) {
                    list.ScrollIntoView(list.Items[lastItem]);
                }
            }
        }
    }
}
