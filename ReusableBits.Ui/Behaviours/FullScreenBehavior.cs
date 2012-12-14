using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace ReusableBits.Ui.Behaviours {
	// from: http://josheinstein.com/blog/index.php/2010/02/fullscreenbehavior-wpf/
	// usage:
	//<Window ... >
    //<i:Interaction.Behaviors>
    //    <Behaviours:FullScreenBehavior
	//				FullScreenOnStartup="False"
    //				FullScreenOnDoubleClick="True"
    //				FullScreenOnMaximize="True"
    //				RestoreOnEscape="True" />
	//
    //</i:Interaction.Behaviors>
	//</Window>

	/// <summary>
	/// A behavior that adds full-screen functionality to a window when the window is maximized,
	/// double clicked, or the escape key is pressed.
	/// </summary>	
	public sealed class FullScreenBehavior : Behavior<Window> {
		private const int WM_SYSCOMMAND = 0x112;
		private const int SC_RESTORE = 0xF120;
		private const int SC_MAXIMIZE = 0xF030;

		private HwndSource mHwndSource;

		public static readonly DependencyProperty FullScreenOnStartupProperty =
			DependencyProperty.Register(
				"FullScreenOnStartup", typeof( bool ), typeof( FullScreenBehavior ), new PropertyMetadata( default( bool )));

		/// <summary>
		/// Whether or not the window should maximize at atartup.
		/// </summary>
		public bool FullScreenOnStartup {
			get { return (bool)GetValue( FullScreenOnStartupProperty ); }
			set { SetValue( FullScreenOnStartupProperty, value ); }
		}

		public static readonly DependencyProperty FullScreenOnMaximizeProperty =
			DependencyProperty.Register(
				"FullScreenOnMaximize", typeof( bool ), typeof( FullScreenBehavior ), new PropertyMetadata( default( bool )));

		/// <summary>
		/// Whether or not user initiated maximizing should put the window into full-screen mode.
		/// </summary>
		public bool FullScreenOnMaximize {
			get { return (bool)GetValue( FullScreenOnMaximizeProperty ); }
			set { SetValue( FullScreenOnMaximizeProperty, value ); }
		}

		public static readonly DependencyProperty FullScreenOnDoubleClickProperty =
			DependencyProperty.Register(
				"FullScreenOnDoubleClick", typeof( bool ), typeof( FullScreenBehavior ), new PropertyMetadata( default( bool )));

		/// <summary>
		/// Whether or not double clicking the window's contents should put the window into full-screen mode.
		/// </summary>
		public bool FullScreenOnDoubleClick {
			get { return (bool)GetValue( FullScreenOnDoubleClickProperty ); }
			set { SetValue( FullScreenOnDoubleClickProperty, value ); }
		}

		public static readonly DependencyProperty RestoreOnEscapeProperty = DependencyProperty.Register(
			"RestoreOnEscape", typeof( bool ), typeof( FullScreenBehavior ), new PropertyMetadata( default( bool )));

		/// <summary>
		/// Whether or not pressing escape while in full screen mode returns to windowed mode.
		/// </summary>
		public bool RestoreOnEscape {
			get { return (bool)GetValue( RestoreOnEscapeProperty ); }
			set { SetValue( RestoreOnEscapeProperty, value ); }
		}

		private static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.RegisterAttached(
			"IsFullScreen", typeof( bool ), typeof( FullScreenBehavior ),
			new PropertyMetadata( default( bool ), null, OnCoerceFullScreenChanged ) );

		/// <summary>
		/// Gets a value indicating whether or not the specified window is currently in full-screen mode.
		/// </summary>
		public static bool GetIsFullScreen( Window window ) {
			return (bool)window.GetValue( IsFullScreenProperty );
		}

		/// <summary>
		/// Sets a value indicating whether or not the specified window is currently in full-screen mode.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="value">The value.</param>
		public static void SetIsFullScreen( Window window, bool value ) {
			window.SetValue( IsFullScreenProperty, value );
		}

		/// <summary>
		/// Called when the value of the IsFullScreenProperty dependency property is to be set.
		/// This method is called whether the value will be changed or not.
		/// </summary>
		/// <param name="sender">The control instance.</param>
		/// <param name="value">The value that is to be set.</param>
		private static object OnCoerceFullScreenChanged( DependencyObject sender, object value ) {
			if(( sender is Window ) &&
			   ( value is bool )) {
				var window = (Window)sender;

				if((bool)value ) {
					window.WindowStyle = WindowStyle.None;
					window.Topmost = true;
					window.WindowState = WindowState.Maximized;
				}
				else {
					window.Topmost = false;
					window.WindowStyle = WindowStyle.SingleBorderWindow;
					window.WindowState = WindowState.Normal;
				}
			}

			return( value );
		}

		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.Loaded += OnLoaded;
			AssociatedObject.SourceInitialized += OnSourceInitialized;
			AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
			AssociatedObject.KeyDown += OnKeyDown;

			AttachHook();
		}

		/// <summary>
		/// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
		/// </summary>
		/// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
		protected override void OnDetaching() {
			DetachHook();

			AssociatedObject.Loaded -= OnLoaded;
			AssociatedObject.SourceInitialized -= OnSourceInitialized;
			AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
			AssociatedObject.KeyDown -= OnKeyDown;

			base.OnDetaching();
		}

		/// <summary>
		/// Adds the hook procedure to the Window's HwndSource.
		/// </summary>
		private void AttachHook() {
			if( mHwndSource == null ) {
				mHwndSource = (HwndSource)PresentationSource.FromVisual( AssociatedObject );
				if( mHwndSource != null ) {
					mHwndSource.AddHook( WndProc );
				} // if
			} // if
		}

		/// <summary>
		/// Removes the hook procedure from the Window's HwndSource.
		/// </summary>
		private void DetachHook() {
			if( mHwndSource != null ) {
				mHwndSource.RemoveHook( WndProc );
				mHwndSource = null;
			} // if
		}

		/// <summary>
		/// A hook procedure that intercepts messages sent to the attached window.
		/// </summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="msg">The message.</param>
		/// <param name="wParam">The wParam which varies by message.</param>
		/// <param name="lParam">The lParam which varies by message.</param>
		/// <param name="handled">Set to true to suppress default process of this message.</param>
		/// <returns>The return value which depends upon the message.</returns>
		private IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled ) {
			if( msg == WM_SYSCOMMAND ) {
				int wParam32 = wParam.ToInt32() & 0xFFF0;
				if( wParam32 == SC_MAXIMIZE ||
				    wParam32 == SC_RESTORE ) {
					if( FullScreenOnMaximize ) {
						// Cancel the default handling
						handled = true;

						// Go to full screen on maximize
						// Return from full screen on restore
						SetIsFullScreen( AssociatedObject, ( wParam32 == SC_MAXIMIZE ) );
					} // if
				} // if
			} // if

			return IntPtr.Zero;
		}

		private void OnLoaded( object sender, RoutedEventArgs args ) {
			if( FullScreenOnStartup ) {
				SetIsFullScreen( AssociatedObject, true );
			}
		}

		/// <summary>
		/// Handles the SourceInitialized event of the Window.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The instance containing the event data.</param>
		private void OnSourceInitialized( object sender, EventArgs e ) {
			AttachHook();
		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the Window.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The instance containing the event data.</param>
		private void OnMouseDoubleClick( object sender, MouseButtonEventArgs e ) {
			if( e.Handled == false ) {
				if( FullScreenOnDoubleClick ) {
					bool current = GetIsFullScreen( AssociatedObject );
					SetIsFullScreen( AssociatedObject, !current );
				} // if
			} // if
		}

		/// <summary>
		/// Handles the KeyDown event of the Window.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The instance containing the event data.</param>
		private void OnKeyDown( object sender, KeyEventArgs e ) {
			if( e.Key == Key.Escape &&
			    e.Handled == false ) {
				if( RestoreOnEscape ) {
					SetIsFullScreen( AssociatedObject, false );
				}
			}
		}
	}
}