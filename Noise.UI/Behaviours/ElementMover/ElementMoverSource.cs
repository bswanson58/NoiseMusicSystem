using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;

namespace Noise.UI.Behaviours.ElementMover {
    public class MoveElementEventArgs : RoutedEventArgs {
        public  FrameworkElement    SourceElement { get; }
        public  string              DisplayText { get; }

        public MoveElementEventArgs( FrameworkElement sourceElement, string displayText ) :
            base( ElementMoverSource.MoveElementEvent ) {
            SourceElement = sourceElement;
            DisplayText = displayText;
        }
    }

    public class ElementMoverSource : Behavior<FrameworkElement> {
        public static readonly RoutedEvent MoveElementEvent = EventManager.RegisterRoutedEvent(
            "MoveElement", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( ElementMoverSource ));

        // Provide CLR accessors for the event
        public event RoutedEventHandler MoveElement {
            add => AssociatedObject.AddHandler( MoveElementEvent, value );
            remove => AssociatedObject.RemoveHandler( MoveElementEvent, value );
        }

        private void RaiseMoveElementEvent() {
            AssociatedObject.RaiseEvent( new MoveElementEventArgs( AssociatedObject, MoveText ));
        }

        public static readonly DependencyProperty FireEventProperty = DependencyProperty.Register(
            "FireEventName", typeof( string ), typeof( ElementMoverSource ), new PropertyMetadata( "MoveElement" ));

        public string FireEventName {
            get => GetValue( FireEventProperty ) as string;
            set => SetValue( FireEventProperty, value );
        }

        public static readonly DependencyProperty MoveTextProperty = DependencyProperty.Register(
            "MoveText", typeof( string ), typeof( ElementMoverSource ), new PropertyMetadata( "Move Text" ));

        public string MoveText {
            get => GetValue( MoveTextProperty ) as string;
            set => SetValue( MoveTextProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.DataContextChanged += OnDataContextChanged;
            ConnectPropertyChanged();
        }

        private void OnDataContextChanged( object sender, DependencyPropertyChangedEventArgs args ) {
            if( args.OldValue is INotifyPropertyChanged source ) {
                source.PropertyChanged -= OnPropertyChanged;
            }

            ConnectPropertyChanged();
        }

        private void ConnectPropertyChanged() {
            if( AssociatedObject.DataContext is INotifyPropertyChanged source ) {
                source.PropertyChanged += OnPropertyChanged;
            }
        }

        protected override void OnDetaching() {
            if( AssociatedObject.DataContext is INotifyPropertyChanged source ) {
                source.PropertyChanged -= OnPropertyChanged;
            }

            AssociatedObject.DataContextChanged -= OnDataContextChanged;

            base.OnDetaching();
        }

        private void OnPropertyChanged( object sender, PropertyChangedEventArgs args ) {
            if( args.PropertyName.Equals( FireEventName )) {
                RaiseMoveElementEvent();
            }
        }
    }
}
