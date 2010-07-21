using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Noise.UI.Behaviours {
	public class AnimateAfterTrigger {
		public static readonly DependencyProperty AnimateToValueProperty =
				DependencyProperty.RegisterAttached( "AnimateToValue",
													 typeof( bool ),
													 typeof( AnimateAfterTrigger ),
													 new UIPropertyMetadata( false, OnAnimateToValue ));

		public static bool GetAnimateToValue( Slider slider ) {
			return((bool)slider.GetValue( AnimateToValueProperty ));
		}

		public static void SetAnimateToValue( Slider slider, bool value) {
			slider.SetValue( AnimateToValueProperty, value);
		}

		static void OnAnimateToValue( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			var slider = depObj as Slider;

			if(( slider != null ) &&
			   ( args.NewValue is bool )) {
				if( (bool)args.NewValue ) {
					slider.Loaded += OnSliderLoaded;
				}
				else {
					slider.Loaded -= OnSliderLoaded;
				}
			}
		}

		static void OnSliderLoaded( object sender, EventArgs args ) {
			var slider = sender as Slider;

			if( slider != null ) {
				var	thumb = FindChild<Thumb>( slider, true );
				if( thumb != null ) {
					thumb.DragCompleted += OnDragCompleted;
				}
			}
		}

	   static void OnDragCompleted( object sender, RoutedEventArgs e ) {
			// Only react to the event raised by the original object.
			if( ReferenceEquals( sender, e.OriginalSource )) {
				var thumb = e.OriginalSource as Thumb;

				if( thumb != null ) {
					var slider = thumb.TemplatedParent as Slider;

					if( slider != null ) {
						slider.Value = 0.0;
					}
				}
			}
		}

        static T FindChild<T>( DependencyObject parent, bool mustBeVisible ) where T : DependencyObject {
            if(( mustBeVisible ) && 
			   ( parent is UIElement ) &&
               ((parent as UIElement).Visibility != Visibility.Visible)) {
                return null;
            }

            if( parent is T ) {
                return parent as T;
			}

            int numChildren = VisualTreeHelper.GetChildrenCount( parent );

            for( int childIndex = 0; childIndex < numChildren; childIndex++ ) {
                var child = FindChild<T>( VisualTreeHelper.GetChild( parent, childIndex ), mustBeVisible );

                if( child != null ) {
                    return child;
				}
            }

            return null;
        }
	}
}
