using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using ReusableBits.Ui.Utility;

namespace ReusableBits.Ui.Behaviours {
    public class WidgetOpacity : Behavior<FrameworkElement> {
        private readonly List<UIElement>    mScrollBars;

        public static readonly DependencyProperty FadeInAnimationProperty = DependencyProperty.Register(
            "FadeInAnimation", typeof( Storyboard ), typeof( WidgetOpacity ), new PropertyMetadata( null ));

        public Storyboard FadeInAnimation {
            get => GetValue( FadeInAnimationProperty ) as Storyboard;
            set => SetValue( FadeInAnimationProperty, value );
        }

        public static readonly DependencyProperty FadeOutAnimationProperty = DependencyProperty.Register(
            "FadeOutAnimation", typeof( Storyboard ), typeof( WidgetOpacity ), new PropertyMetadata( null ));

        public Storyboard FadeOutAnimation {
            get => GetValue( FadeOutAnimationProperty ) as Storyboard;
            set => SetValue( FadeOutAnimationProperty, value );
        }

        public WidgetOpacity() {
            mScrollBars = new List<UIElement>();
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        private void OnLoaded( object sender, RoutedEventArgs args ) {
            mScrollBars.AddRange( AssociatedObject.FindChildren<ScrollBar>());

            FadeOutWidgets();
        }

        private void OnMouseEnter( object sender, MouseEventArgs args ) {
            var animation = FadeInAnimation;

            if( animation != null ) {
                foreach( var element in mScrollBars ) {
                    Storyboard.SetTarget( animation, element );
                    animation.Begin();
                }
            }
        }

        private void OnMouseLeave( object sender, MouseEventArgs args ) {
            FadeOutWidgets();
        }

        private void FadeOutWidgets() {
            var animation = FadeOutAnimation;

            if( animation != null ) {
                foreach( var element in mScrollBars ) {
                    Storyboard.SetTarget( animation, element );
                    animation.Begin();
                }
            }
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.MouseEnter -= OnMouseEnter;
            AssociatedObject.MouseLeave -= OnMouseLeave;

            mScrollBars.Clear();
        }
    }
}
