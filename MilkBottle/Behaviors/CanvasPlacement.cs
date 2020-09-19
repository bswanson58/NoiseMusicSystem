using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace MilkBottle.Behaviors {
    public class CanvasPlacement : Behavior<FrameworkElement>{
        public static readonly DependencyProperty CanvasProperty = DependencyProperty.Register(
            "Canvas",
            typeof( Canvas ),
            typeof( CanvasPlacement ),
            new PropertyMetadata( null ));

        public Canvas Canvas {
            get => (Canvas)GetValue( CanvasProperty );
            set => SetValue( CanvasProperty, value );
        }

        public static readonly DependencyProperty ClampToCanvasProperty = DependencyProperty.Register(
            "ClampToCanvas",
            typeof( bool ),
            typeof( CanvasPlacement ),
            new PropertyMetadata( null ));

        public bool ClampToCanvas {
            get => (bool)GetValue( ClampToCanvasProperty );
            set => SetValue( ClampToCanvasProperty, value );
        }

        public static readonly DependencyProperty PlacementTopProperty = DependencyProperty.Register(
            "PlacementTop",
            typeof( double ),
            typeof( CanvasPlacement ),
            new PropertyMetadata( OnPlacementChanged ));

        public double PlacementTop {
            get => (double)GetValue( PlacementTopProperty );
            set => SetValue( PlacementTopProperty, value );
        }

        public static readonly DependencyProperty PlacementLeftProperty = DependencyProperty.Register(
            "PlacementLeft",
            typeof( double ),
            typeof( CanvasPlacement ),
            new PropertyMetadata( OnPlacementChanged ));

        public double PlacementLeft {
            get => (double)GetValue( PlacementLeftProperty );
            set => SetValue( PlacementLeftProperty, value );
        }

        public static readonly DependencyProperty PlacementXOffsetProperty = DependencyProperty.Register(
            "PlacementXOffset",
            typeof( double ),
            typeof( CanvasPlacement ),
            new PropertyMetadata( OnPlacementChanged ));

        public double PlacementXOffset {
            get => (double)GetValue( PlacementXOffsetProperty );
            set => SetValue( PlacementXOffsetProperty, value );
        }

        public static readonly DependencyProperty PlacementYOffsetProperty = DependencyProperty.Register(
            "PlacementYOffset",
            typeof( double ),
            typeof( CanvasPlacement ),
            new PropertyMetadata( OnPlacementChanged ));

        public double PlacementYOffset {
            get => (double)GetValue( PlacementYOffsetProperty );
            set => SetValue( PlacementYOffsetProperty, value );
        }

        private static void OnPlacementChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
            if( sender is CanvasPlacement target ) {
                target.UpdatePlacement();
            }
        }

        private void UpdatePlacement() {
            if( Canvas != null ) {
                var top = PlacementTop + PlacementYOffset;
                var left = PlacementLeft + PlacementXOffset;

                if( ClampToCanvas ) {
                    if(( top + AssociatedObject.ActualHeight ) > Canvas.ActualHeight ) {
                        top = Canvas.ActualHeight - AssociatedObject.ActualHeight;
                    }
                    if( top < 0 ) {
                        top = 0;
                    }

                    if(( left + AssociatedObject.ActualWidth ) > Canvas.ActualWidth ) {
                        left = Canvas.ActualWidth - AssociatedObject.ActualWidth;
                    }
                    if( left < 0 ) {
                        left = 0;
                    }
                }

                Canvas.SetTop( AssociatedObject, top );
                Canvas.SetLeft( AssociatedObject, left );
            }
        }
    }
}
