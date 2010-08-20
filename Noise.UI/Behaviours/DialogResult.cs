using System.Windows;

namespace Noise.UI.Behaviours {
	public class DialogCloser {
		public static readonly DependencyProperty DialogResultProperty =
			DependencyProperty.RegisterAttached(
				"DialogResult",
				typeof( bool? ),
				typeof( DialogCloser ),
				new PropertyMetadata( DialogResultChanged ));

		private static void DialogResultChanged(
			DependencyObject d,
			DependencyPropertyChangedEventArgs e ) {
			var window = d as Window;

			if(( window != null ) &&
			   ( window.IsInitialized )) {
				window.DialogResult = e.NewValue as bool?;
			}
		}
		public static void SetDialogResult( Window target, bool? value ) {
			target.SetValue( DialogResultProperty, value );
		}
	}
}
