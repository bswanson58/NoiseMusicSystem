using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Noise.UI.Behaviours {
	public class SliderAnimateAfterDrag : System.Windows.Interactivity.Behavior<Slider> {
		private Slider	mSlider;
		private Thumb	mThumb;

		public static readonly DependencyProperty StoryBoardProperty =
				DependencyProperty.RegisterAttached( "Storyboard",
													 typeof( Storyboard ),
													 typeof( SliderAnimateAfterDrag ),
													 new FrameworkPropertyMetadata( null ));

		public Storyboard Storyboard {
			get { return ( (Storyboard)GetValue( StoryBoardProperty ) ); }
			set { SetValue( StoryBoardProperty, value ); }
		}


		protected override void OnAttached() {
			base.OnAttached();

			mSlider = AssociatedObject;
			if( mSlider != null ) {
				mSlider.Loaded += OnSliderLoaded;
			}
		}

		protected override void OnDetaching() {
			base.OnDetaching();

			if( mSlider != null ) {
				mSlider.Loaded -= OnSliderLoaded;

				mSlider = null;
			}

			if( mThumb != null ) {
				mThumb.DragCompleted -= OnDragCompleted;

				mThumb = null;
			}
		}

		private void OnSliderLoaded( object sender, RoutedEventArgs args ) {
			if( mSlider != null ) {
				mThumb = FindChild<Thumb>( mSlider, true );

				if( mThumb != null ) {
					mThumb.DragCompleted += OnDragCompleted;
				}
			}
		}

		private void OnDragCompleted( object sender, RoutedEventArgs e ) {
			// Only react to the event raised by the original object.
			if(( ReferenceEquals( sender, e.OriginalSource )) &&
			   ( Storyboard != null )) {
				Storyboard.Completed += OnStoryboardCompleted;
				Storyboard.Begin();
			}
		}

		private void OnStoryboardCompleted( object sender, System.EventArgs e ) {
			Storyboard.Completed -= OnStoryboardCompleted;
			mSlider.BeginAnimation( RangeBase.ValueProperty, null );
			mSlider.Value = 0.0;
		}

		static T FindChild<T>( DependencyObject parent, bool mustBeVisible ) where T : DependencyObject {
			if(( mustBeVisible ) &&
			   ( parent is UIElement ) &&
			   (( parent as UIElement ).Visibility != Visibility.Visible )) {
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
