using System.Windows;
using System.Windows.Controls;

namespace MilkBottle.Controls {
    public class CustomPlacementCanvas : Canvas {
        protected override Size ArrangeOverride( Size arrangeSize ) {
            foreach( UIElement child in InternalChildren ) {
                var canvasPoint = ToCanvas( GetTop( child ), GetLeft( child ));

                child.Arrange( new Rect( canvasPoint, child.DesiredSize ));
            }

            return arrangeSize;
        }

        Point ToCanvas( double top, double left ) {
            var x = ActualWidth / 1 * left;
            var y = ActualHeight / 1 * top;

            return new Point( x, y );
        }
    }
}
