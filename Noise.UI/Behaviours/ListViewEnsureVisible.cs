using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Behaviours {
	public class ListViewEnsureVisible {
		public static int GetVisibleIndex( ListBox listView ) {
			return (int)listView.GetValue( VisibleIndexProperty );
		}

		public static void SetVisibleIndex( ListBox listView, int value ) {
			listView.SetValue( VisibleIndexProperty, value );
		}

		public static readonly DependencyProperty VisibleIndexProperty =
			DependencyProperty.RegisterAttached(
				"VisibleIndex",
				typeof( int ),
				typeof( ListViewEnsureVisible ),
				new UIPropertyMetadata( -1, VisibleIndexChanged ));

		static void VisibleIndexChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			var list = depObj as ListBox;

			if(( list != null ) &&
			   ( args.NewValue is int )) {
				var index = (int)args.NewValue;
				if(( index > -1 ) &&
				   ( list.Items.Count > index )) {
					var item = list.Items[index];

					list.ScrollIntoView( item );
				}
			}
		}
	}
}
