using System.Windows;
using System.Windows.Input;

namespace ReusableBits.Ui.Behaviours {
	public class ViewAttachedNotifier : DependencyObject {
		public static readonly DependencyProperty NotifyCommandProperty =
			DependencyProperty.RegisterAttached( "NotifyCommand", typeof( ICommand ), typeof( ViewAttachedNotifier ), new PropertyMetadata( null, OnNotifyCommandChanged ));

		public static void SetNotifyCommand( DependencyObject d, ICommand value ) {
			d.SetValue( NotifyCommandProperty, value );
		}
		public static ICommand GetNotifyCommand( DependencyObject d ) {
			return( (ICommand)d.GetValue( NotifyCommandProperty ));
		}

		private static void OnNotifyCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			if( d is FrameworkElement ) {
				(d as FrameworkElement).Loaded += OnElementLoaded; 
			}
		}

		private static void OnElementLoaded( object sender, RoutedEventArgs args ) {
			if( sender is FrameworkElement ) {
				var parent = sender as FrameworkElement;

				parent.Loaded -= OnElementLoaded;

				var notifyCommand = GetNotifyCommand( parent );

				if( notifyCommand != null ) {
					notifyCommand.Execute( null );
				}
			}
		}
	}
}
