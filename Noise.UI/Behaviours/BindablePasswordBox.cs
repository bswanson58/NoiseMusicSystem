using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Behaviours {
	public class BindablePasswordBox : Decorator {
		/// <summary>
		/// The password dependency property.
		/// </summary>
		public	static readonly DependencyProperty	PasswordProperty;
		private	bool								mIsPreventCallback;
		private	readonly RoutedEventHandler			mSavedCallback;

		/// <summary>
		/// Static constructor to initialize the dependency properties.
		/// </summary>
		static BindablePasswordBox() {
			PasswordProperty = DependencyProperty.Register(
				"BindablePassword",
				typeof( string ),
				typeof( BindablePasswordBox ),
				new FrameworkPropertyMetadata( "", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged ));
		}

		/// <summary>
		/// Saves the password changed callback and sets the child element to the password box.
		/// </summary>
		public BindablePasswordBox() {
			mSavedCallback = HandlePasswordChanged;

			var passwordBox = new PasswordBox();
			passwordBox.PasswordChanged += mSavedCallback;
			Child = passwordBox;
		}

		/// <summary>
		/// The password dependency property.
		/// </summary>
		public string BindablePassword {
			get { return GetValue( PasswordProperty ) as string; }
			set { SetValue( PasswordProperty, value ); }
		}

		/// <summary>
		/// Handles changes to the password dependency property.
		/// </summary>
		/// <param name="d">the dependency object</param>
		/// <param name="eventArgs">the event args</param>
		private static void OnPasswordPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs eventArgs ) {
			var bindablePasswordBox = (BindablePasswordBox)d;
			var	passwordBox = (PasswordBox)bindablePasswordBox.Child;

			if( bindablePasswordBox.mIsPreventCallback ) {
				return;
			}

			passwordBox.PasswordChanged -= bindablePasswordBox.mSavedCallback;
			passwordBox.Password = ( eventArgs.NewValue != null ) ? eventArgs.NewValue.ToString() : "";
			passwordBox.PasswordChanged += bindablePasswordBox.mSavedCallback;
		}

		/// <summary>
		/// Handles the password changed event.
		/// </summary>
		/// <param name="sender">the sender</param>
		/// <param name="eventArgs">the event args</param>
		private void HandlePasswordChanged( object sender, RoutedEventArgs eventArgs ) {
			var passwordBox = (PasswordBox)sender;

			mIsPreventCallback = true;
			BindablePassword = passwordBox.Password;
			mIsPreventCallback = false;
		}
	}
}
