using System.Windows;
using System.ComponentModel;

namespace Noise.UI.Behaviours.EventCommandTriggers {
	public class PropertyCommandTrigger : CommandTrigger {
		/// <value>Identifies the Property dependency property</value>
		public static readonly DependencyProperty PropertyProperty =
			DependencyProperty.Register( "Property", typeof( DependencyProperty ), typeof( PropertyCommandTrigger ),
			new FrameworkPropertyMetadata( null ) );

		/// <value>description for Property property</value>
		public DependencyProperty Property {
			get { return (DependencyProperty)GetValue( PropertyProperty ); }
			set { SetValue( PropertyProperty, value ); }
		}

		/// <value>Identifies the Value dependency property</value>
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register( "Value", typeof( string ), typeof( PropertyCommandTrigger ),
			new FrameworkPropertyMetadata( null ) );

		/// <value>description for Value property</value>
		public string Value {
			get { return (string)GetValue( ValueProperty ); }
			set { SetValue( ValueProperty, value ); }
		}

		protected override Freezable CreateInstanceCore() {
			return new PropertyCommandTrigger();
		}

		/// <value>Identifies the T dependency property</value>
		public static readonly DependencyProperty TProperty =
			DependencyProperty.Register( "T", typeof( object ), typeof( PropertyCommandTrigger ),
			new FrameworkPropertyMetadata( null, OnTChanged ) );

		/// <value>description for T property</value>
		public object T {
			get { return GetValue( TProperty ); }
			set { SetValue( TProperty, value ); }
		}

		/// <summary>
		/// Invoked on T change.
		/// </summary>
		/// <param name="d">The object that was changed</param>
		/// <param name="e">Dependency property changed event arguments</param>
		static void OnTChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
		}

		protected override void InitializeCore( FrameworkElement source ) {
			DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty( Property, source.GetType() );
			descriptor.AddValueChanged( source, ( s, e ) => {
				CommandParameter<object> parameter = new PropertyCommandParameter<object, object>(
					CustomParameter, Property, source.GetValue( Property ));

				object value = Value;
				if(( descriptor.Converter != null ) &&
				   ( descriptor.Converter.CanConvertFrom( typeof( string )))) {
					value = descriptor.Converter.ConvertFromString( Value );
				}

				if( Equals( source.GetValue( Property ), value ) )
					ExecuteCommand( parameter );
			} );
		}
	}
}
