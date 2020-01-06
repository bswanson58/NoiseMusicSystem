using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using ReusableBits.Ui.Utility;

namespace ReusableBits.Ui.Behaviours {
    public class WidgetOpacity : Behavior<FrameworkElement> {
        private readonly List<FrameworkElement> mElements;

        public static readonly DependencyProperty TagNameProperty = DependencyProperty.Register(
            "TagName", typeof( string ), typeof( WidgetOpacity ), new PropertyMetadata( null, OnTagNameChanged ));

        private static void OnTagNameChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
            if( sender is WidgetOpacity widget ) {
                widget.OnTagNameChanged();
            }
        }

        public string TagName {
            get => GetValue( TagNameProperty ) as string;
            set => SetValue( TagNameProperty, value );
        }

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
            mElements = new List<FrameworkElement>();
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        private void OnLoaded( object sender, RoutedEventArgs args ) {
            LoadElements();

            FadeOutWidgets();
        }

        public void OnTagNameChanged() {
            LoadElements();
        }

        private void LoadElements() {
            if( AssociatedObject != null ) {
                mElements.Clear();

                mElements.AddRange( AssociatedObject.FindChildren<FrameworkElement>().Where( c => c.Tag != null && c.Tag.Equals( TagName )));
            }
        }

        private void OnMouseEnter( object sender, MouseEventArgs args ) {
            var animation = FadeInAnimation;

            if( animation != null ) {
                foreach( var element in mElements ) {
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
                foreach( var element in mElements ) {
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

            mElements.Clear();
        }
    }
}
