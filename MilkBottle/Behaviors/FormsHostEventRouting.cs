using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using ReusableBits.Ui.Utility;
using Application = System.Windows.Application;
using Control = System.Windows.Forms.Control;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace MilkBottle.Behaviors {
    public class FormsHostEventRouting : Behavior<WindowsFormsHost> {
        public static readonly DependencyProperty RouteMouseWheelEventsProperty = DependencyProperty.Register(
            "RouteMouseWheelEvents",
            typeof( bool ),
            typeof( FormsHostEventRouting ),
            new PropertyMetadata( null ) );

        public bool RouteMouseWheelEvents {
            get => (bool)GetValue( RouteMouseWheelEventsProperty );
            set => SetValue( RouteMouseWheelEventsProperty, value );
        }

        public static readonly DependencyProperty RouteToElementProperty = DependencyProperty.Register(
            "RouteToElement",
            typeof( FrameworkElement ),
            typeof( FormsHostEventRouting ),
            new PropertyMetadata( null ) );

        public FrameworkElement RouteToElement {
            get => GetValue( RouteToElementProperty ) as FrameworkElement;
            set => SetValue( RouteToElementProperty, value );
        }

        public static readonly DependencyProperty RouteToTagProperty = DependencyProperty.Register(
            "RouteToTag",
            typeof( String ),
            typeof( FormsHostEventRouting ),
            new PropertyMetadata( null ) );

        public String RouteToTag {
            get => (string)GetValue( RouteToTagProperty );
            set => SetValue( RouteToTagProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.ChildChanged += OnChildChanged;
        }

        protected override void OnDetaching() {
            AssociatedObject.ChildChanged -= OnChildChanged;

            base.OnDetaching();
        }

        private void OnChildChanged( object sender, ChildChangedEventArgs args ) {
            if( args.PreviousChild is Control control ) {
                control.MouseWheel -= OnMouseWheel;
            }

            if( AssociatedObject.Child != null ) {
                AssociatedObject.Child.MouseWheel += OnMouseWheel;
            }
        }

        private void OnMouseWheel( object sender, MouseEventArgs args ) {
            var wheelEvent = new MouseWheelEventArgs( Mouse.PrimaryDevice, DateTime.Now.Millisecond, args.Delta ) { RoutedEvent = UIElement.MouseWheelEvent, Source = sender };

            if( RouteToElement != null ) {
                RouteToElement.RaiseEvent( wheelEvent );
            }
            else if( !String.IsNullOrWhiteSpace( RouteToTag ) ) {
                var element = Application.Current.MainWindow.FindChildren<FrameworkElement>().FirstOrDefault( c => c.Tag?.Equals( RouteToTag ) == true );

                element?.RaiseEvent( wheelEvent );
            }
            else {
                AssociatedObject.RaiseEvent( wheelEvent );
            }
        }
    }
}
