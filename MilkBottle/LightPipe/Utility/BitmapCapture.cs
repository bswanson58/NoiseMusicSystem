using System;
using System.Drawing;
using LightPipe.Platform;

namespace LightPipe.Utility {
    public class BitmapCapture {
        public static Bitmap Capture( IntPtr hWnd ) {
            var retValue = default( Bitmap );
            var deviceContext = Win32API.GetDC( hWnd );

            if( deviceContext != IntPtr.Zero ) {
                Win32API.GetWindowRect( hWnd, out var rect );

                if(!Win32API.IsRectEmpty( ref rect )) {
                    retValue = new Bitmap( rect.Width, rect.Height );

                    using( var destGraphics = Graphics.FromImage( retValue )) {
                        Win32API.BitBlt( destGraphics.GetHdc(), 0, 0, rect.Width, rect.Height, deviceContext, 0, 0, TernaryRasterOperations.SRCCOPY );
                    }
                }

                Win32API.ReleaseDC( hWnd, deviceContext );
            }

            return retValue;
        }
    }
}
