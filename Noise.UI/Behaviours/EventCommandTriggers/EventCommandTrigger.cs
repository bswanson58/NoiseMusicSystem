using System.Windows;

// from: http://blogs.microsoft.co.il/blogs/tomershamam/archive/2009/04/14/wpf-commands-everywhere.aspx

namespace Noise.UI.Behaviours.EventCommandTriggers {
	public sealed class EventCommandTrigger : CommandTrigger {
		/// <value>Identifies the RoutedEvent dependency property</value>
		public static readonly DependencyProperty RoutedEventProperty =
			DependencyProperty.Register( "RoutedEvent", typeof( RoutedEvent ), typeof( EventCommandTrigger ),
			new FrameworkPropertyMetadata( null ) );

		/// <value>description for RoutedEvent property</value>
		public RoutedEvent RoutedEvent {
			get { return (RoutedEvent)GetValue( RoutedEventProperty ); }
			set { SetValue( RoutedEventProperty, value ); }
		}

		protected override Freezable CreateInstanceCore() {
			return new EventCommandTrigger();
		}

		protected override void InitializeCore( FrameworkElement source ) {
			source.AddHandler( RoutedEvent, (RoutedEventHandler)ExecuteCommand );
		}

		private void ExecuteCommand( object sender, RoutedEventArgs args ) {
			CommandParameter<object> parameter = new EventCommandParameter<object, RoutedEventArgs>( CustomParameter, RoutedEvent, args );

			ExecuteCommand( parameter );
		}
	}
}
