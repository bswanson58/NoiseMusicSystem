using System.Windows;

namespace Noise.UI.Behaviours.EventCommandTriggers {
	public static class CommandSource {
		public static ICommandTrigger GetTrigger( FrameworkElement source ) {
			return (ICommandTrigger)source.GetValue( TriggerProperty );
		}

		public static void SetTrigger( FrameworkElement source, ICommandTrigger value ) {
			source.SetValue( TriggerProperty, value );
		}

		// Using a DependencyProperty as the backing store for Trigger.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TriggerProperty =
			DependencyProperty.RegisterAttached(
				"Trigger",
				typeof( ICommandTrigger ),
				typeof( CommandSource ),
				new UIPropertyMetadata(
					null,
					TriggerPropertyChanged ) );

		private static void TriggerPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			var element = d as FrameworkElement;

			var commandTrigger = e.NewValue as ICommandTrigger;
			if( commandTrigger != null ) {
				commandTrigger.Initialize( element );
			}
		}
	}
}
