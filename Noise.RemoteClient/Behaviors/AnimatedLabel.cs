using System;
using System.Threading.Tasks;
using Xamarin.Forms;

// Usage:
//    <Label ... > 
//      <Label.Behaviors>
//          <behaviors:AnimatedLabel Text="{Binding ...}" AnimationLength="500" AnimationType="ScaleY"/>
//      <Label.Behaviors>
//    <Label>

namespace Noise.RemoteClient.Behaviors {
    public enum AnimationType {
        Fade,
        ScaleX,
        ScaleY
    }
    
    class AnimatedLabel : Behavior<Label> {
        public Label        AssociatedObject { get; private set; }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create( nameof( Text ), typeof( string ), typeof( AnimatedLabel ), String.Empty, propertyChanged: OnTextChanged );

        public string Text {
            get => (string)GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }

        public static readonly BindableProperty AnimationTypeProperty =
            BindableProperty.Create( nameof( AnimationType ), typeof( AnimationType ), typeof( AnimatedLabel ), AnimationType.Fade );

        public AnimationType AnimationType {
            get => (AnimationType)GetValue( AnimationTypeProperty );
            set => SetValue( AnimationTypeProperty, value );
        }

        public static readonly BindableProperty AnimationLengthProperty =
            BindableProperty.Create( nameof( AnimationLength ), typeof( uint ), typeof( AnimatedLabel ), (uint)250 );

        public uint AnimationLength {
            get => (uint)GetValue( AnimationLengthProperty );
            set => SetValue( AnimationLengthProperty, value );
        }

        protected override void OnAttachedTo( Label associatedObject ) {
            base.OnAttachedTo( associatedObject );

            AssociatedObject = associatedObject;

            if( associatedObject.BindingContext != null ) {
                BindingContext = associatedObject.BindingContext;
            }

            associatedObject.BindingContextChanged += OnBindingContextChanged;
        }

        private void OnBindingContextChanged( object sender, EventArgs e ) {
            OnBindingContextChanged();
        }

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();

            BindingContext = AssociatedObject.BindingContext;
        }

        protected override void OnDetachingFrom( Label associatedObject ) {
            base.OnDetachingFrom( associatedObject );

            if( AssociatedObject != null ) {
                AssociatedObject.BindingContextChanged -= OnBindingContextChanged;
            }
        }

        private static void OnTextChanged( BindableObject sender, Object oldValue, object newValue ) {
            if(( sender is AnimatedLabel label ) &&
               ( newValue is string newText )) {
                label.AnimateChange( newText );
            }
        }

        private async void AnimateChange( string newValue ) {
            await AnimateOut();

            AssociatedObject.Text = newValue;

            await AnimateIn();
        }

        private async Task AnimateOut() {
            switch( AnimationType ) {
                case AnimationType.Fade:
                    await AssociatedObject.FadeTo( 0.0, AnimationLength, Easing.Linear );
                    break;

                case AnimationType.ScaleX:
                    await AssociatedObject.ScaleXTo( 0.0, AnimationLength, Easing.SinIn );
                    break;

                case AnimationType.ScaleY:
                    await AssociatedObject.ScaleYTo( 0.0, AnimationLength, Easing.SinIn );
                    break;
            }
        }

        private async Task AnimateIn() {
            switch( AnimationType ) {
                case AnimationType.Fade:
                    await AssociatedObject.FadeTo( 1.0, AnimationLength, Easing.Linear );
                    break;

                case AnimationType.ScaleX:
                    await AssociatedObject.ScaleXTo( 1.0, AnimationLength, Easing.SinOut );
                    break;

                case AnimationType.ScaleY:
                    await AssociatedObject.ScaleYTo( 1.0, AnimationLength, Easing.SinOut );
                    break;
            }
        }
    }
}
