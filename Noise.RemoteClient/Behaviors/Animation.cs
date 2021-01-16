using System;
using Xamarin.Forms;

// Usage:
//    <Label ... > 
//      <Label.Behaviors>
//          <behaviors:Animation AnimationState="{Binding ...}" AnimationStyle="TranslateX"
//                               TrueAmount="100" TrueLength="400" TrueEasing="{x:Static Easing.BounceOut}"
//                               FalseAmount="0" FalseLength="500" TrueEasing="{x:Static Easing.BounceIn}"/>
//      <Label.Behaviors>
//    <Label>
namespace Noise.RemoteClient.Behaviors {
    public enum AnimationStyle {
        Fade,
        Rotate,
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

        public static readonly BindableProperty TrueLengthProperty =
            BindableProperty.Create( nameof( TrueLength ), typeof( uint ), typeof( Animation ), (uint)0 );

        public uint TrueLength {
            get => (uint)GetValue( TrueLengthProperty );
            set => SetValue( TrueLengthProperty, value );
        }

        public static readonly BindableProperty FalseLengthProperty =
            BindableProperty.Create( nameof( FalseLength ), typeof( uint ), typeof( Animation ), (uint)0 );

        public uint FalseLength {
            get => (uint)GetValue( FalseLengthProperty );
            set => SetValue( FalseLengthProperty, value );
        }

        public static readonly BindableProperty AnimationLengthProperty =
            BindableProperty.Create( nameof( AnimationLength ), typeof( uint ), typeof( Animation ), (uint)250 );

        public uint AnimationLength {
            get => (uint)GetValue( AnimationLengthProperty );
            set => SetValue( AnimationLengthProperty, value );
        }

        public static readonly BindableProperty TrueEasingProperty =
            BindableProperty.Create( nameof( TrueEasing ), typeof( Easing ), typeof( Animation ), Easing.Linear );

        public Easing TrueEasing {
            get => (Easing)GetValue( TrueEasingProperty );
            set => SetValue( TrueEasingProperty, value );
        }

        public static readonly BindableProperty FalseEasingProperty =
            BindableProperty.Create( nameof( FalseEasing ), typeof( Easing ), typeof( Animation ), Easing.Linear );

        public Easing FalseEasing {
            get => (Easing)GetValue( FalseEasingProperty );
            set => SetValue( FalseEasingProperty, value );
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
                case AnimationStyle.Fade:
                    Fade();
                    break;

                case AnimationStyle.Rotate:
                    Rotate();
                    break;

                case AnimationStyle.TranslateX:
                    TranslateX();
                    break;

                case AnimationStyle.TranslateY:
                    TranslateY();
                    break;
            }
        }

        private void Fade() {
            mAssociatedObject.FadeTo( AnimationState ? TrueAmount : FalseAmount, GetLength(), AnimationState ? TrueEasing : FalseEasing );
        }

        private void Rotate() {
            mAssociatedObject.RotateTo( AnimationState ? TrueAmount : FalseAmount, GetLength(), AnimationState ? TrueEasing : FalseEasing );
        }

        private void TranslateX() {
            mAssociatedObject.TranslateTo( AnimationState ? TrueAmount : FalseAmount, 0.0D, GetLength(), AnimationState ? TrueEasing : FalseEasing );
        }

        private void TranslateY() {
            mAssociatedObject.TranslateTo( 0.0D, AnimationState ? TrueAmount : FalseAmount, GetLength(), AnimationState ? TrueEasing : FalseEasing );
        }

        private uint GetLength() {
            var retValue = AnimationLength;

            if(( TrueLength != 0 ) ||
               ( FalseLength != 0 )) {
                retValue = AnimationState ? TrueLength : FalseLength;
            }

            return retValue;
        }

        protected override void OnDetachingFrom( View associatedObject ) {
            base.OnDetachingFrom( associatedObject );

            if( mAssociatedObject != null ) {
                mAssociatedObject.BindingContextChanged -= OnBindingContextChanged;
            }
        }
    }
}
