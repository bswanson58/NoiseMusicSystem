using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Point = System.Drawing.Point;

namespace ReusableBits.Ui.Utility {
    public class ExecutingEnvironment {
        public static void ResizeWindowIntoVisibility( Window window ) {
            var     graphics = Graphics.FromHwnd( new WindowInteropHelper( window ).Handle);
            float   scaleX;
            float   scaleY;

            try {
                scaleX = graphics.DpiX / 96.0f;
                scaleY = graphics.DpiY / 96.0f;
            }
            finally {
                graphics.Dispose();
            }

            var rect = new Rectangle((int)( window.Left * scaleX ), (int)( window.Top * scaleY ),
                                     (int)( window.Width * scaleX ), (int)( window.Height * scaleY ));

            if(!IsOnScreen( rect, 1.0 )) {
                var screen = Screen.FromRectangle( rect );

                if( rect.Right > screen.WorkingArea.Right ) {
                    rect.Width = Math.Min( rect.Width, screen.WorkingArea.Width );
                    rect.Location = new Point( screen.WorkingArea.Right - rect.Width ,rect.Top );
                }
                if( rect.Left < screen.WorkingArea.Left ) {
                    rect.Width = Math.Min( rect.Width, screen.WorkingArea.Width );
                    rect.Location = new Point( screen.WorkingArea.Left, rect.Top );
                }

                if( rect.Bottom > screen.WorkingArea.Bottom ) {
                    rect.Height = Math.Min( rect.Height, screen.WorkingArea.Height );
                    rect.Location = new Point( rect.Left, screen.WorkingArea.Bottom - rect.Height );
                }
                if( rect.Top < screen.WorkingArea.Top ) {
                    rect.Height = Math.Min( rect.Height, screen.WorkingArea.Height );
                    rect.Location = new Point( rect.Left, screen.WorkingArea.Top );
                }

                window.Top = rect.Top / scaleY;
                window.Height = rect.Height / scaleY;
                window.Left = rect.Left / scaleX;
                window.Width = rect.Width / scaleX;
            }
        }

        public static void MoveWindowIntoVisibility( Window window ) {
            var     graphics = Graphics.FromHwnd( new WindowInteropHelper( window ).Handle);
            float   scaleX;
            float   scaleY;

            try {
                scaleX = graphics.DpiX / 96.0f;
                scaleY = graphics.DpiY / 96.0f;
            }
            finally {
                graphics.Dispose();
            }

            var rect = new Rectangle((int)( window.Left * scaleX ), (int)( window.Top * scaleY ),
                                     (int)( window.Width * scaleX ), (int)( window.Height * scaleY ));

            if(!IsOnScreen( rect, 0.8 )) {
                var screen = Screen.FromRectangle( rect );

                var left = Math.Max( rect.Left, screen.WorkingArea.Left );
                var top = Math.Max( rect.Top, screen.WorkingArea.Top );

                left = Math.Min( left, ( screen.WorkingArea.Left + screen.WorkingArea.Width ) - rect.Width );
                top = Math.Min( top, ( screen.WorkingArea.Top + screen.WorkingArea.Height ) - rect.Height );

                window.Left = left / scaleX;
                window.Top = top / scaleY;
            }
        }

        // Return True if at least the percent specified of the rectangle is shown on the total screen area of all monitors, otherwise return False.
        private static bool IsOnScreen( Rectangle rect, double minimumPercentOnScreen ) {
            double pixelsVisible = 0;

            foreach( var screen in Screen.AllScreens ) {
                var intersect = Rectangle.Intersect( rect, screen.WorkingArea );

                if(( intersect.Width != 0 ) &&
                   ( intersect.Height != 0 )) {
                    pixelsVisible += ( intersect.Width * intersect.Height );
                }
            }

            return pixelsVisible >= (( rect.Width * rect.Height ) * minimumPercentOnScreen );
        }
    }
}

