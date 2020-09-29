﻿using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace MilkBottle.Behaviors {
    class StoryboardManager : Behavior<FrameworkElement> {
        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register(
            "Storyboard",
            typeof( Storyboard ),
            typeof( StoryboardManager ),
            new PropertyMetadata( null ));

        public Storyboard Storyboard {
            get => GetValue( StoryboardProperty ) as Storyboard;
            set => SetValue( StoryboardProperty, value );
        }

        public static readonly DependencyProperty TriggerAnimationProperty = DependencyProperty.Register(
            "TriggerAnimation",
            typeof( object ),
            typeof( StoryboardManager ),
            new PropertyMetadata( OnTriggerAnimation ));

        public static void OnTriggerAnimation( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
            if( sender is StoryboardManager manager ) {
                manager.StartAnimation();
            }
        }

        public object TriggerAnimation {
            get => GetValue( TriggerAnimationProperty );
            set => SetValue( TriggerAnimationProperty, value );
        }

        public static readonly DependencyProperty TargetElementNameProperty = DependencyProperty.Register(
            "TargetElementName",
            typeof( string ),
            typeof( StoryboardManager ),
            new PropertyMetadata( null ));

        public string TargetElementName {
            get => (string)GetValue( TargetElementNameProperty );
            set => SetValue( TargetElementNameProperty, value );
        }

        private void StartAnimation() {
            if(( AssociatedObject != null ) &&
               ( Storyboard != null )) {
                if(!String.IsNullOrWhiteSpace( TargetElementName )) {
                    Storyboard.SetTargetName( Storyboard, TargetElementName );
                }

                Storyboard.Begin( AssociatedObject, HandoffBehavior.SnapshotAndReplace );
            }
        }
    }
}