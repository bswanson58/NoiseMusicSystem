using System;
using Caliburn.Micro;
using LightPipe.Interfaces;
using LightPipe.Utility;

namespace LightPipe.Models {
    public class LightPipeController : ILightPipeController, IHandle<Events.FrameRendered> {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IImageProcessor    mImageProcessor;
        private bool                        mEnabled;
        private int                         mCaptureFrequency;
        private DateTime                    mLastCaptureTime;

        public LightPipeController( IImageProcessor imageProcessor, IEventAggregator eventAggregator ) {
            mImageProcessor = imageProcessor;
            mEventAggregator = eventAggregator;

            mEnabled = false;
            mCaptureFrequency = 100;

            mEventAggregator.Subscribe( this );
        }

        public void Initialize() {
            EnableLightPipe( true );
        }

        public void EnableLightPipe( bool state ) {
            mEnabled = state;

            mLastCaptureTime = DateTime.Now;
        }

        public void SetCaptureFrequency( int milliseconds ) {
            mCaptureFrequency = milliseconds;
        }

        public void Handle( Events.FrameRendered args ) {
            if(( mEnabled ) &&
               ( mLastCaptureTime + TimeSpan.FromMilliseconds( mCaptureFrequency ) < DateTime.Now )) {

                CaptureFrame( args.WindowPtr );

                mLastCaptureTime = DateTime.Now;
            }
        }

        private void CaptureFrame( IntPtr hWnd ) {
            var bitmap = BitmapCapture.Capture( hWnd );

            if( bitmap != null ) {
                mImageProcessor.ProcessImage( bitmap );
            }
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }
    }
}
