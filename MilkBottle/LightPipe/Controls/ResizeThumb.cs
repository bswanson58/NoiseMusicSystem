using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LightPipe.Controls {
    public class ResizeThumb : Thumb {
        public ResizeThumb() {
            DragDelta += ResizeThumb_DragDelta;
        }

        private void ResizeThumb_DragDelta( object sender, DragDeltaEventArgs e ) {
            if( DataContext is FrameworkElement designerItem ) {
                double deltaVertical, deltaHorizontal;

                switch( VerticalAlignment ) {
                    case VerticalAlignment.Top:
                        deltaVertical = Math.Min( e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight );
                        deltaHorizontal = Canvas.GetBottom( designerItem );
                        Canvas.SetTop( designerItem, Canvas.GetTop( designerItem ) + deltaVertical );
                        Canvas.SetBottom( designerItem, deltaHorizontal );
                        break;

                    case VerticalAlignment.Bottom:
                        deltaVertical = Math.Min( -e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight );
                        Canvas.SetBottom( designerItem, Canvas.GetBottom( designerItem ) - deltaVertical );
                        break;
                }

                switch( HorizontalAlignment ) {
                    case HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min( e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth );
                        deltaVertical = Canvas.GetRight( designerItem );
                        Canvas.SetLeft( designerItem, Canvas.GetLeft( designerItem ) + deltaHorizontal );
                        Canvas.SetRight( designerItem, deltaVertical );
                        break;

                    case HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min( -e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth );
                        Canvas.SetRight( designerItem, Canvas.GetRight( designerItem ) - deltaHorizontal );
                        break;
                }
            }

            e.Handled = true;
        }
    }
}
