using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using LightPipe.Utility;
using MilkBottle.Entities;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class PresetImageHandler : IPresetImageHandler, IHandle<LightPipe.Events.FrameRendered> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IBasicLog              mLog;
        private readonly ManualResetEventSlim   mCaptureEvent;
        private byte[]                          mDefaultImage;
        private Bitmap                          mCapturedBitmap;

        public PresetImageHandler( IEventAggregator eventAggregator, IBasicLog log ) {
            mEventAggregator = eventAggregator;
            mLog = log;

            mCaptureEvent = new ManualResetEventSlim();
        }

        public byte[] GetPresetImage( Preset preset ) {
            var retValue = default( byte[] );
            var imagePath = Path.ChangeExtension( preset.Location, ".jpg" );

            if( !String.IsNullOrWhiteSpace( imagePath ) ) {
                if( File.Exists( imagePath ) ) {
                    using( var stream = File.OpenRead( imagePath ) ) {
                        retValue = new byte[stream.Length];

                        stream.Read( retValue, 0, retValue.Length );
                        stream.Close();
                    }
                }
                else {
                    retValue = GetDefaultImage();
                }
            }

            return retValue;
        }

        public async Task<byte[]> CapturePresetImage( Preset preset ) {
            await WaitForCapture( preset );

            return GetPresetImage( preset );
        }

        private Task<bool> WaitForCapture( Preset preset ) {
            return Task.Run( () => {
                var retValue = false;

                mCapturedBitmap?.Dispose();
                mCapturedBitmap = null;
                mCaptureEvent.Reset();
                mEventAggregator.Subscribe( this );

                if( mCaptureEvent.Wait( TimeSpan.FromMilliseconds( 10000 ))) {
                     if( mCapturedBitmap != null ) {
                         try {
                             var imagePath = Path.ChangeExtension( preset.Location, ".jpg" );

                             if(!String.IsNullOrWhiteSpace( imagePath )) {
                                 mCapturedBitmap.Save( imagePath, ImageFormat.Jpeg );
                             }

                             mCapturedBitmap.Dispose();
                             mCapturedBitmap = null;

                             retValue = true;
                         }
                         catch( Exception ex ) {
                             mLog.LogException( "CapturePresetImage", ex );
                         }
                     }
                }
                else {
                    mEventAggregator.Unsubscribe( this );
                }

                return retValue;
            });
        }

        public void Handle( LightPipe.Events.FrameRendered args ) {
            mEventAggregator.Unsubscribe( this );

            try {
                using( var bitmap = BitmapCapture.Capture( args.WindowPtr )) {
                    if( bitmap != null ) {
                        mCapturedBitmap = ResizeImage( bitmap, 160, 90 );
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "CapturePresetImage", ex );
            }
            finally {
                mCaptureEvent.Set();
            }
        }

        public static Bitmap ResizeImage( Image image, int width, int height ) {
            var destRect = new Rectangle( 0, 0, width, height );
            var destImage = new Bitmap( width, height );

            destImage.SetResolution( image.HorizontalResolution, image.VerticalResolution );

            using( var graphics = Graphics.FromImage( destImage ) ) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using( var wrapMode = new ImageAttributes() ) {
                    wrapMode.SetWrapMode( WrapMode.TileFlipXY );
                    graphics.DrawImage( image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode );
                }
            }

            return destImage;
        }

        private byte[] GetDefaultImage() {
            if( mDefaultImage == null ) {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                using( var stream = assembly.GetManifestResourceStream( assembly.GetName().Name + ".Resources.Default Preset Image.png" ) ) {
                    if( stream != null ) {
                        mDefaultImage = new byte[stream.Length];

                        stream.Read( mDefaultImage, 0, mDefaultImage.Length );
                    }
                }
            }

            return mDefaultImage;
        }
    }
}
