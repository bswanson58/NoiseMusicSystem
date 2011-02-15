using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Behaviours {
	public class ScrollViewerFirstItemVisible {
		public static object GetChangeTrigger( ScrollViewer scroller ) {
			return( scroller.GetValue( ChangeTriggerProperty ));
		}

		public static void SetChangeTrigger( ScrollViewer scroller, object value ) {
			scroller.SetValue( ChangeTriggerProperty, value );
		}

		public static readonly DependencyProperty ChangeTriggerProperty =
			DependencyProperty.RegisterAttached(
				"ChangeTrigger",
				typeof( object ),
				typeof( ScrollViewerFirstItemVisible ),
				new UIPropertyMetadata( null, OnChangeTrigger ) );

		static void OnChangeTrigger( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			var scroller = depObj as ScrollViewer;

			if( scroller != null ) {
				scroller.ScrollToHome();
			}
		}

	}
}
