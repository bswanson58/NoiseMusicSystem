using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Behaviours {

	public class ListViewFirstItemVisible {
		public static int GetChangeTrigger( ListView listView ) {
			return (int)listView.GetValue( ChangeTriggerProperty );
		}

		public static void SetChangeTrigger( ListView listView, int value ) {
			listView.SetValue( ChangeTriggerProperty, value );
		}

		public static readonly DependencyProperty ChangeTriggerProperty =
			DependencyProperty.RegisterAttached(
				"ChangeTrigger",
				typeof( object ),
				typeof( ListViewFirstItemVisible ),
				new UIPropertyMetadata( null, OnChangeTrigger ));

		static void OnChangeTrigger( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			var list = depObj as ListView;

			if(( list != null ) &&
			   ( list.Items.Count > 0 )) {
				list.ScrollIntoView( list.Items[0]);
			}
		}
	}
}
