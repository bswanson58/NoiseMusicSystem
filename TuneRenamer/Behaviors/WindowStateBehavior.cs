using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace TuneRenamer.Behaviors {
    class WindowStateBehavior : Behavior<Window> {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register( "Command", typeof( ICommand ), typeof( WindowStateBehavior )); 

        public ICommand Command {
            get => (ICommand)GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.StateChanged += OnStateChanged;
        }

        private void OnStateChanged( object sender, EventArgs args ) {
            Command.Execute( AssociatedObject.WindowState );
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            AssociatedObject.StateChanged -= OnStateChanged;
        }
    }
}
