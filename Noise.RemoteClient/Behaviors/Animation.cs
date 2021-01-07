using System;
using Xamarin.Forms;

// Usage:
//    <Label ... > 
//      <Label.Behaviors>
//          <behaviors:Animation AnimationStyle="TranslateX" TrueAmount="100" FalseAmount="0" AnimationLength="500" AnimationState="{Binding ...}"/>
//      <Label.Behaviors>
//    <Label>
namespace Noise.RemoteClient.Behaviors {
    public enum AnimationStyle {
        Fade,
        ScaleX,
        ScaleY,
        TranslateX,
        TranslateY
    }

    class Animation : Behavior<View> {
        private View    mAssociatedObject;

        public static readonly BindableProperty AnimationStyleProperty =
            BindableProperty.Create( nameof( AnimationStyle ), typeof( AnimationStyle ), typeof( Animation ), AnimationStyle.Fade );

        public AnimationStyle AnimationStyle {
            get => (AnimationStyle)GetValue( AnimationStyleProperty );
            set => SetValue( AnimationStyleProperty, value );
        }

        public static readonly BindableProperty FalseAmountProperty =
            BindableProperty.Create( nameof( FalseAmount ), typeof( double ), typeof( Animation ), 0.0D );

        public double FalseAmount {
            get => (double)GetValue( FalseAmountProperty );
            set => SetValue( FalseAmountProperty, value );
        }

        public static readonly BindableProperty TrueAmountProperty =
            BindableProperty.Create( nameof( TrueAmount ), typeof( double ), typeof( Animation ), 0.0D );

        public double TrueAmount {
            get => (double)GetValue( TrueAmountProperty );
            set => SetValue( TrueAmountProperty, value );
        }

        public static readonly BindableProperty AnimationLengthProperty =
            BindableProperty.Create( nameof( AnimationLength ), typeof( uint ), typeof( Animation ), (uint)250 );

        public uint AnimationLength {
            get => (uint)GetValue( AnimationLengthProperty );
            set => SetValue( AnimationLengthProperty, value );
        }

        public static readonly BindableProperty AnimationStateProperty =
            BindableProperty.Create( nameof( AnimationState ), typeof( bool ), typeof( Animation ), false, propertyChanged: OnStateChanged );

        public bool AnimationState {
            get => (bool)GetValue( AnimationStateProperty );
            set => SetValue( AnimationStateProperty, value );
        }

        protected override void OnAttachedTo( View associatedObject ) {
            base.OnAttachedTo( associatedObject );

            mAssociatedObject = associatedObject;

            if( associatedObject.BindingContext != null ) {
                BindingContext = associatedObject.BindingContext;
            }

            mAssociatedObject.BindingContextChanged += OnBindingContextChanged;
        }

        private void OnBindingContextChanged( object sender, EventArgs e ) {
            OnBindingContextChanged();
        }

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();

            BindingContext = mAssociatedObject.BindingContext;
        }

        private static void OnStateChanged( BindableObject sender, Object oldValue, object newValue ) {
            if(( sender is Animation animation ) &&
               ( newValue is bool )) {
                animation.StateChanged();
            }
        }

        private void StateChanged() {
            switch( AnimationStyle ) {
                case AnimationStyle.TranslateX:
                    TranslateX();
                    break;
            }
        }

        private void TranslateX() {
            mAssociatedObject.TranslateTo( AnimationState ? TrueAmount : FalseAmount, 0.0D, AnimationLength );
        }

        protected override void OnDetachingFrom( View associatedObject ) {
            base.OnDetachingFrom( associatedObject );

            if( mAssociatedObject != null ) {
                mAssociatedObject.BindingContextChanged -= OnBindingContextChanged;
            }
        }
    }
}
