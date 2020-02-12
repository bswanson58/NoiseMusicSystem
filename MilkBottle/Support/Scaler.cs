using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MilkBottle.Support {
    public class Scaler {
        private static Scaler  mGlobalScaler;

        private readonly float  mScaleX;
        private readonly float  mScaleY;

        public float            ScaleX { get { return( mScaleX ); } }
        public float            ScaleY { get { return( mScaleY ); } }

        private Scaler() {
            var window = Process.GetCurrentProcess().MainWindowHandle;

            var graphics = Graphics.FromHwnd( window );
            try {
                mScaleX = graphics.DpiX / 96.0f;
                mScaleY = graphics.DpiY / 96.0f;
            }
            finally {
                graphics.Dispose();
            }
        }

        public static Scaler Current {
            get {
                if( mGlobalScaler == null ) {
                    mGlobalScaler = new Scaler();
                }

                return( mGlobalScaler );
            }
        }

        public Image ScaleImage( Image image ) {
            var retValue = image;

            if(( image != null ) &&
               ( mScaleX > 1.0f ) &&
               ( mScaleY > 1.0f )) {
                var destRect = new Rectangle( 0, 0, (int)( image.Width * mScaleX ), (int)( image.Height * mScaleY ));
                var destImage = new Bitmap( destRect.Width, destRect.Height );

                destImage.SetResolution( image.HorizontalResolution, image.VerticalResolution );

                using( var graphics = Graphics.FromImage( destImage )) {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using( var wrapMode = new ImageAttributes()) {
                        wrapMode.SetWrapMode( WrapMode.TileFlipXY );
                        graphics.DrawImage( image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode );
                    }
                }

                retValue = destImage;
            }

            return( retValue );
        }

	    public int ScaleHeight( int height ) {
		    return ( int )( height * mScaleY );
	    }

	    public int ScaleWidth( int width ) {
		    return ( int )( width * mScaleX );
	    }

	    public Size ScaleSize( Size size ) {
            return ( new Size( (int)( size.Width * mScaleX ), (int)( size.Height * mScaleY )));
        }
	}
}
