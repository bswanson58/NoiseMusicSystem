using System.Windows;
using System.Windows.Input;

namespace ReusableBits.Ui.Behaviours {
	public class ViewDisplayedNotifier : DependencyObject {
		public static readonly DependencyProperty NotifyCommandProperty =
			DependencyProperty.RegisterAttached( "NotifyCommand", typeof( ICommand ), typeof( ViewDisplayedNotifier ), new PropertyMetadata( null, OnNotifyCommandChanged ));

		public static void SetNotifyCommand( DependencyObject d, ICommand value ) {
			d.SetValue( NotifyCommandProperty, value );
		}
		public static ICommand GetNotifyCommand( DependencyObject d ) {
			return ( (ICommand)d.GetValue( NotifyCommandProperty ) );
		}

		private static void OnNotifyCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			if( d is FrameworkElement ) {
				var element = d as FrameworkElement;

				element.GotFocus += OnFocus;
				element.LostFocus += OnLostFocus;
			}
		}

		private static void OnFocus( object sender, RoutedEventArgs args ) {
			if( sender is FrameworkElement ) {
				NotifyFocus( sender as FrameworkElement, true );
			}
		}

		private static void OnLostFocus( object sender, RoutedEventArgs args ) {
			if( sender is FrameworkElement ) {
				NotifyFocus( sender as FrameworkElement, false );
			}
		}

		private static void NotifyFocus( FrameworkElement sender, bool isFocused ) {
			var notifyCommand = GetNotifyCommand( sender );

			if( notifyCommand != null ) {
				notifyCommand.Execute( isFocused );
			}
		}
	}
}
