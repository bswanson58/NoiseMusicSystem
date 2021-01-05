﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;

// Usage:
//    <Label ... > 
//      <Label.Behaviors>
//          <behaviors:DelayedLabel Text="{Binding ...}" DelayLength="5000" AnimationLength="500"/>
//      <Label.Behaviors>
//    <Label>

namespace Noise.RemoteClient.Behaviors {
    class DelayedLabel : Behavior<Label> {
        public Label    AssociatedObject { get; private set; }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create( nameof( Text ), typeof( string ), typeof( DelayedLabel ), String.Empty, propertyChanged: OnTextChanged );

        public string Text {
            get => (string)GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }

        public static readonly BindableProperty DelayLengthProperty =
            BindableProperty.Create( nameof( DelayLength ), typeof( int ), typeof( DelayedLabel ), 3000 );

        public int DelayLength {
            get => (int)GetValue( DelayLengthProperty );
            set => SetValue( DelayLengthProperty, value );
        }

        public static readonly BindableProperty AnimationLengthProperty =
            BindableProperty.Create( nameof( AnimationLength ), typeof( uint ), typeof( DelayedLabel ), (uint)250 );

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
            if(( sender is DelayedLabel label ) &&
               ( newValue is string newText )) {
                label.AnimateChange( newText );
            }
        }

        private async void AnimateChange( string newValue ) {
            await AssociatedObject.FadeTo( 0.0, 0 );
            await Task.Delay( DelayLength );

            AssociatedObject.Text = newValue;

            await AssociatedObject.FadeTo( 1.0, AnimationLength, Easing.Linear );
        }
    }
}
