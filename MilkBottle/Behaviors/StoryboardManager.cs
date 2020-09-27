using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace MilkBottle.Behaviors {
    class StoryboardManager : Behavior<FrameworkElement> {
        public static readonly DependencyProperty StoryboardsProperty = DependencyProperty.Register(
            "Storyboards",
            typeof( Storyboard[] ),
            typeof( StoryboardManager ),
            new PropertyMetadata( null ));

        public Storyboard[] Storyboards {
            get => (Storyboard[])GetValue( StoryboardsProperty );
            set => SetValue( StoryboardsProperty, value );
        }

        public static readonly DependencyProperty ParentProperty = DependencyProperty.Register(
            "Parent",
            typeof( FrameworkElement ),
            typeof( StoryboardManager ),
            new PropertyMetadata( null ));

        public FrameworkElement Parent {
            get => GetValue( ParentProperty ) as FrameworkElement;
            set => SetValue( ParentProperty, value );
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
            var animation = Storyboards.FirstOrDefault();

            if(( animation != null ) &&
               (!String.IsNullOrWhiteSpace( TargetElementName ))) {
                Storyboard.SetTargetName( animation, TargetElementName );

                animation.Begin( AssociatedObject, HandoffBehavior.SnapshotAndReplace );
            }
        }
    }
}
