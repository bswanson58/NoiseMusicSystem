using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LightPipe.Controls {
    public class MoveThumb : Thumb {
        public MoveThumb() {
            DragDelta += MoveThumb_DragDelta;
        }

        private void MoveThumb_DragDelta( object sender, DragDeltaEventArgs e ) {
            if( DataContext is FrameworkElement designerItem ) {
                Canvas.SetLeft( designerItem, Canvas.GetLeft( designerItem ) + e.HorizontalChange );
                Canvas.SetTop( designerItem, Canvas.GetTop( designerItem ) + e.VerticalChange );
            }
        }
    }
}
