using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace MilkBottle.Behaviors {
    public class VisualStateController : Behavior<Control> {
        public static readonly DependencyProperty DefaultStateProperty = DependencyProperty.Register(
            "DefaultState",
            typeof( string ),
            typeof( VisualStateController ),
            new PropertyMetadata( null ));

        public string DefaultState {
            get => (string)GetValue( DefaultStateProperty );
            set => SetValue( DefaultStateProperty, value );
        }

        public static readonly DependencyProperty VisualStateProperty = DependencyProperty.Register(
            "VisualState",
            typeof( string ),
            typeof( VisualStateController ),
            new PropertyMetadata( OnTriggerState ));

        public static void OnTriggerState( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
            if( sender is VisualStateController manager ) {
                manager.GoToState();
            }
        }

        public string VisualState {
            get => (string)GetValue( VisualStateProperty );
            set => SetValue( VisualStateProperty, value );
        }

        private void GoToState() {
            if( AssociatedObject != null ) {
                if((!String.IsNullOrWhiteSpace( DefaultState )) &&
                   (!VisualState.Equals( DefaultState ))) {
                    VisualStateManager.GoToState( AssociatedObject, DefaultState, true );
                }

                VisualStateManager.GoToState( AssociatedObject, StartUntil( VisualState, '|' ), true );
            }
        }

        private static string StartUntil( string input, char stopChar ) {
            var retValue = input;

            if(!String.IsNullOrWhiteSpace( input )) {
                retValue = String.Concat( input.TakeWhile( c => c != stopChar ));
            }

            return retValue;
        }
    }
}
