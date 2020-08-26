using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using LightPipe.Dto;

namespace LightPipe.Utility {
    public class DirectAccessBitmap : IDisposable {
        public Bitmap Bitmap;
        public readonly BitmapData Data;
        public readonly int PixelSize;
        public readonly int Left;
        public readonly int Top;
        public readonly int Width;
        public readonly int Height;
        public readonly int Stride;
        public readonly IntPtr Scan0;

        public DirectAccessBitmap( Bitmap bitmap ) :
            this( bitmap, 0, 0, bitmap.Width, bitmap.Height ) {
        }

        public DirectAccessBitmap( Bitmap bitmap, int left, int top, int width, int height ) {
            Bitmap = bitmap;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Data = Bitmap.LockBits( new Rectangle( left, top, width, height ), ImageLockMode.ReadWrite, bitmap.PixelFormat );
            PixelSize = FindPixelSize();
            Stride = Data.Stride;
            Scan0 = Data.Scan0;
        }

        public IEnumerable<PixelData> CopyPixels( int startRow, int startColumn, int height, int width ) {
            var retValue = new List<PixelData>();

            unsafe {
                var pixelSize = PixelSize;

                for( var rowIndex = startRow; rowIndex < startRow + Height; rowIndex++ ) {
                    for( int column = startColumn; column < startColumn + Width; column += 1 ) {
                        var pixelPointer = (byte *)Scan0 + ( rowIndex * Stride ) + ( column * pixelSize );

                        // *(pixelPointer + 0) is B (Blue ) component of the pixel
                        // *(pixelPointer + 1) is G (Green) component of the pixel
                        // *(pixelPointer + 2) is R (Red  ) component of the pixel
                        // *(pixelPointer + 3) is A (Alpha) component of the pixel ( for 32bpp )
                        retValue.Add( new PixelData( rowIndex, column, *(pixelPointer), *(pixelPointer + 1), *(pixelPointer + 2), *(pixelPointer + 3 )));
                    }
                }
            }

            return retValue;
        }

        public IEnumerable<PixelData> SamplePixels( int sampleSize ) {
            return SamplePixels( 0, 0, Height, Width, sampleSize );
        }

        public IEnumerable<PixelData> SamplePixels( int startRow, int startColumn, int height, int width, int sampleSize ) {
            var retValue = new List<PixelData>();

            unsafe {
                for( var rowIndex = startRow; rowIndex < startRow + Height; rowIndex += sampleSize ) {
                    for( int column = startColumn; column < startColumn + Width; column += sampleSize ) {
                        var pixelPointer = (byte *)Scan0 + ( rowIndex * Stride ) + ( column * PixelSize );

                        retValue.Add( new PixelData( rowIndex, column, *(pixelPointer), *(pixelPointer + 1), *(pixelPointer + 2), *(pixelPointer + 3 )));
                    }
                }
            }

            return retValue;
        }

        public void Dispose() {
            Bitmap.UnlockBits( Data );
        }

        private int FindPixelSize() {
            if( Data.PixelFormat == PixelFormat.Format24bppRgb ) {
                return 3;
            }
            if( Data.PixelFormat == PixelFormat.Format32bppArgb ) {
                return 4;
            }

            return 4;
        }
    }
}
