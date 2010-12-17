using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Behaviours {
	public class ScrollViewerPosition {
		public static object GetChangeTrigger( ScrollViewer scrollView ) {
			return( scrollView.GetValue( ChangeTriggerProperty ));
		}

		public static void SetChangeTrigger( ScrollViewer scrollView, object value ) {
			scrollView.SetValue( ChangeTriggerProperty, value );
		}

		public static readonly DependencyProperty ChangeTriggerProperty =
			DependencyProperty.RegisterAttached(
				"ChangeTrigger",
				typeof( object ),
				typeof( ScrollViewerPosition ),
				new UIPropertyMetadata( null, OnChangeTrigger ));

		static void OnChangeTrigger( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			var scroller = depObj as ScrollViewer;

			if( scroller != null ) {
				scroller.ScrollToVerticalOffset( 0.0 );
			}
		}
	}
}
