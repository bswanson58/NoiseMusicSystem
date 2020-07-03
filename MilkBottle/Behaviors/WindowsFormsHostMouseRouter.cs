using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interactivity;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace MilkBottle.Behaviors {
    // from: https://stackoverflow.com/questions/26909257/keep-mouse-events-bubbling-from-windowsformshost-on

    // <i:Interaction.Behaviors>
    //  <behaviors:WindowsFormsHostMouseRouter/>
    // </i:Interaction.Behaviors>

    class WindowsFormsHostMouseRouter : Behavior<WindowsFormsHost> {
        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.ChildChanged += OnChildChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            AssociatedObject.ChildChanged -= OnChildChanged;
        }

        private void OnChildChanged(object sender, ChildChangedEventArgs childChangedEventArgs) {
            if( childChangedEventArgs.PreviousChild is Control previousChild ) {
                previousChild.MouseDown -= OnMouseDown;
                previousChild.MouseEnter -= OnMouseEnter;
                previousChild.MouseMove -= OnMouseMove;
                previousChild.MouseLeave -= OnMouseLeave;
            }

            if( AssociatedObject.Child != null) {
                AssociatedObject.Child.MouseDown += OnMouseDown;
                AssociatedObject.Child.MouseMove += OnMouseMove;
                AssociatedObject.Child.MouseEnter += OnMouseEnter;
                AssociatedObject.Child.MouseLeave += OnMouseLeave;
            }
        }

        private void OnMouseEnter( object sender, EventArgs args ) {
            AssociatedObject.RaiseEvent( new System.Windows.Input.MouseEventArgs( Mouse.PrimaryDevice, 0 ) { RoutedEvent = Mouse.MouseEnterEvent, Source = this });
        }

        private void OnMouseMove( object sender, MouseEventArgs args ) {
            AssociatedObject.RaiseEvent( new System.Windows.Input.MouseEventArgs( Mouse.PrimaryDevice, 0 ) { RoutedEvent = Mouse.MouseMoveEvent, Source = this });
        }
        
        private void OnMouseLeave( object sender, EventArgs args ) {
            AssociatedObject.RaiseEvent( new System.Windows.Input.MouseEventArgs( Mouse.PrimaryDevice, 0 ) { RoutedEvent = Mouse.MouseLeaveEvent, Source = this });
        }

        private void OnMouseDown( object sender, MouseEventArgs mouseEventArgs ) {
            MouseButton? wpfButton = ConvertToWpf( mouseEventArgs.Button );

            if(!wpfButton.HasValue ) {
                return;
            }

            AssociatedObject.RaiseEvent( new MouseButtonEventArgs( Mouse.PrimaryDevice, 0, wpfButton.Value ) {
                RoutedEvent = Mouse.MouseDownEvent,
                Source = this,
            });
        }

        private MouseButton? ConvertToWpf( MouseButtons winformButton ) {
            switch( winformButton ) {
                case MouseButtons.Left:
                    return MouseButton.Left;
                case MouseButtons.None:
                    return null;
                case MouseButtons.Right:
                    return MouseButton.Right;
                case MouseButtons.Middle:
                    return MouseButton.Middle;
                case MouseButtons.XButton1:
                    return MouseButton.XButton1;
                case MouseButtons.XButton2:
                    return MouseButton.XButton2;
                default:
                    throw new ArgumentOutOfRangeException( "winformButton" );
            }
        }
    }
}
