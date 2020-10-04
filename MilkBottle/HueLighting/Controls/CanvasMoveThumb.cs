using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HueLighting.Controls {
    public class HorizontalCanvasMoveThumb : CanvasMoveThumb {
        public HorizontalCanvasMoveThumb() : base( false, true ) { }
    }

    public class VerticalCanvasMoveThumb : CanvasMoveThumb {
        public VerticalCanvasMoveThumb() : base( true, false ) { }
    }

    public class CanvasMoveThumb : Thumb {
        private readonly bool   mSetLeft;
        private readonly bool   mSetTop;

        public CanvasMoveThumb() {
            DragDelta += MoveThumb_DragDelta;

            mSetTop = true;
            mSetLeft = true;
        }

        public CanvasMoveThumb( bool setTop, bool setLeft ) :
            this() {
            mSetTop = setTop;
            mSetLeft = setLeft;
        }

        private void MoveThumb_DragDelta( object sender, DragDeltaEventArgs e ) {
            if( DataContext is FrameworkElement designerItem ) {
                if( mSetLeft ) {
                    Canvas.SetLeft( designerItem, Canvas.GetLeft( designerItem ) + e.HorizontalChange );
                }

                if( mSetTop ) {
                    Canvas.SetTop( designerItem, Canvas.GetTop( designerItem ) + e.VerticalChange );
                }
            }
        }
    }
}
