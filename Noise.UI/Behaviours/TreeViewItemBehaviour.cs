using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Behaviours {
	public class TreeViewItemBehaviour {
		public static bool GetIsBroughtIntoViewWhenSelected( TreeViewItem treeViewItem ) {
			return (bool)treeViewItem.GetValue( IsBroughtIntoViewWhenSelectedProperty );
		}

		public static void SetIsBroughtIntoViewWhenSelected( TreeViewItem treeViewItem, bool value ) {
			treeViewItem.SetValue( IsBroughtIntoViewWhenSelectedProperty, value );
		}

		public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
			DependencyProperty.RegisterAttached(
				"IsBroughtIntoViewWhenSelected",
				typeof( bool ),
				typeof( TreeViewItemBehaviour ),
				new UIPropertyMetadata( false, OnIsBroughtIntoViewWhenSelectedChanged ) );

		static void OnIsBroughtIntoViewWhenSelectedChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs e ) {
			var item = depObj as TreeViewItem;

			if(( item != null ) &&
			   ( e.NewValue is bool )) {
				if( (bool)e.NewValue ) {
					item.Selected += OnItemSelected;
				}
				else {
					item.Selected -= OnItemSelected;
				}
			}
		}

		static void OnItemSelected( object sender, RoutedEventArgs e ) {
			var item = e.OriginalSource as TreeViewItem;

			if( item != null ) {
				item.BringIntoView();
			}
		}
	}
}
