using System;
using System.Drawing;
using LightPipe.Platform;

namespace LightPipe.Utility {
    public class BitmapCapture : IDisposable {
        private Bitmap  mBitmap;

        public Bitmap CaptureDoNotDispose( IntPtr hWnd ) {
            var deviceContext = Win32API.GetDC( hWnd );

            if( deviceContext != IntPtr.Zero ) {
                Win32API.GetWindowRect( hWnd, out var rect );

                if(!Win32API.IsRectEmpty( ref rect )) {
                    if(( mBitmap == null ) ||
                       ( mBitmap.Width != rect.Width || mBitmap.Height != rect.Height )) {
                        mBitmap?.Dispose();
                        mBitmap = new Bitmap( rect.Width, rect.Height );
                    }

                    using( var destGraphics = Graphics.FromImage( mBitmap )) {
                        Win32API.BitBlt( destGraphics.GetHdc(), 0, 0, rect.Width, rect.Height, deviceContext, 0, 0, TernaryRasterOperations.SRCCOPY );
                    }
                }

                Win32API.ReleaseDC( hWnd, deviceContext );
            }

            return mBitmap;
        }

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

        public void Dispose() {
            mBitmap?.Dispose();
            mBitmap = null;
        }
    }
}
