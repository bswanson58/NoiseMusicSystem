using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ReusableBits.Ui.Behaviours {
	// This behavior changes a DependencyObjects property by fading the Opacity of the element to 0,
	// updates the property and then raises the opactiy back to 1.
	// The Binding property should be set to the original property binding.
	// The Property should be set to the name of the target property to be set.
	// The Duration property may be used to set the fade in and fade out time.
	// The TransitionDelay property may be used to define a delay between the fade out and fade in.
	//
	// Typical usage:
	//
	//		<TextBlock ns:FadedPropertyChange.Property="Text"
	//				   ns:FadedPropertyChange.Duration="0:0:0.7"
	//				   ns:FadedPropertyChange.TransitionDelay="0:0:0.3"
	//				   ns:FadedPropertyChange.Binding="{Binding TextProperty}"/>
	//
	// from: http://stackoverflow.com/questions/1263001/wpf-text-fade-out-then-in-effect

	public class FadedPropertyChange : DependencyObject {
		public static DependencyProperty BindingProperty = 
			DependencyProperty.RegisterAttached( "Binding", typeof( object ), typeof( FadedPropertyChange ), new PropertyMetadata( BindingChanged ));

		public static object GetBinding( DependencyObject dependencyObject ) {
			return dependencyObject.GetValue( BindingProperty );
		}
		public static void SetBinding( DependencyObject dependencyObject, object value ) {
			dependencyObject.SetValue( BindingProperty, value );
		}

		public static DependencyProperty PropertyProperty = 
			DependencyProperty.RegisterAttached( "Property", typeof( string ), typeof( FadedPropertyChange ));

		public static string GetProperty( DependencyObject dependencyObject ) {
			return (string)dependencyObject.GetValue( PropertyProperty );
		}
		public static void SetProperty( DependencyObject dependencyObject, string value ) {
			dependencyObject.SetValue( PropertyProperty, value );
		}

		public static readonly DependencyProperty DurationProperty =
			DependencyProperty.RegisterAttached( "Duration", typeof( TimeSpan ), typeof( FadedPropertyChange ),
													new FrameworkPropertyMetadata( TimeSpan.FromMilliseconds( 500 ), null, CoerceDuration ));
		private static object CoerceDuration( DependencyObject dependencyObject, object newValue ) {
			var retValue = newValue;

			if(( dependencyObject != null ) &&
			   ( newValue is TimeSpan )) {
				var duration = (TimeSpan)newValue;

				if( duration < GetTransitionDelay( dependencyObject )) {
					retValue = GetTransitionDelay( dependencyObject );
				}
			}

			return ( retValue );
		}

		public static void SetDuration( DependencyObject dependencyObject, TimeSpan value ) {
			dependencyObject.SetValue( DurationProperty, value );
		}
		public static TimeSpan GetDuration( DependencyObject dependencyObject ) {
			return (TimeSpan)dependencyObject.GetValue( DurationProperty );
		}

		public static readonly DependencyProperty TransitionDelayProperty = 
			DependencyProperty.RegisterAttached( "TransitionDelay", typeof( TimeSpan ), typeof( FadedPropertyChange ),
													new FrameworkPropertyMetadata( TimeSpan.FromMilliseconds( 100 ), null, CoerceDelay ));
		private static object CoerceDelay( DependencyObject dependencyObject, object newValue ) {
			var retValue = newValue;

			if(( dependencyObject != null ) &&
			   ( newValue is TimeSpan )) {
				var delay = (TimeSpan)newValue;

				if( delay > GetDuration( dependencyObject )) {
					retValue = GetDuration( dependencyObject );
				}
			}

			return ( retValue );
		}

		public static void SetTransitionDelay( DependencyObject dependencyObject, TimeSpan value ) {
			dependencyObject.SetValue( TransitionDelayProperty, value );
		}
		public static TimeSpan GetTransitionDelay( DependencyObject dependencyObject ) {
			return (TimeSpan)dependencyObject.GetValue( TransitionDelayProperty );
		}

		// When the value changes do the fadeout-switch-fadein
		private static void BindingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			if(( GetBinding( d ) != null ) &&
			   ( d.GetType().GetProperty( GetProperty( d )) != null ) ) {
				var fadeout = new Storyboard();
				var fadeoutAnim = new DoubleAnimation { To = 0, Duration = GetDuration( d ), AccelerationRatio = 0.1 };

				Storyboard.SetTarget( fadeoutAnim, d );
				Storyboard.SetTargetProperty( fadeoutAnim, new PropertyPath( "Opacity" ));
				fadeout.Children.Add( fadeoutAnim );
				fadeout.Completed += ( d1, d2 ) => {
					try {
						d.GetType().GetProperty( GetProperty( d )).SetValue( d, GetBinding( d ), null );
					}
					catch( Exception ex ) {
						throw new Exception( string.Format( "FadedPropertyChange: Exception changing '{0}' property.", GetProperty( d )), ex );
					}

					var fadein = new Storyboard();
					var fadeinAnim = new DoubleAnimation { To = 1, Duration = GetDuration( d ), BeginTime = GetTransitionDelay( d ), AccelerationRatio = 0.9 };

					Storyboard.SetTarget( fadeinAnim, d );
					Storyboard.SetTargetProperty( fadeinAnim, new PropertyPath( "Opacity" ));
					fadein.Children.Add( fadeinAnim );
					fadein.Begin();
				};

				fadeout.Begin();
			}
		}
	}
}
