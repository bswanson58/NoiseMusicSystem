using System;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Models;
using MilkBottle.Support;
using OpenTK;

namespace MilkBottle.ViewModels {
    public class GlSurfaceViewModel : IDisposable, IHandle<Events.ApplicationClosing> {
        private readonly IEventAggregator   mEventAggregator;
        private readonly DispatcherTimer    mRenderTimer;
        private readonly ProjectMWrapper    mProjectM;
        private readonly AudioManager       mAudio;
        private GLControl                   mGlControl;

        public GlSurfaceViewModel( IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;

            mProjectM = new ProjectMWrapper();
            mAudio = new AudioManager();

            mRenderTimer = new DispatcherTimer( DispatcherPriority.Normal ) { Interval = TimeSpan.FromMilliseconds( 20 ) };
            mRenderTimer.Tick += OnTimer;

            mEventAggregator.Subscribe( this );
        }

        private void OnPresetSwitched(bool isHardCut, ulong presetIndex ) {
            var presetName = mProjectM.getPresetName( presetIndex );
            var presetLocation = mProjectM.getPresetURL( presetIndex );
        }

        public void Initialize( GLControl glControl ) {
            mGlControl = glControl;

            mAudio.InitializeAudioCapture();
            mGlControl.MakeCurrent();

            mProjectM.initialize(  @"D:\projectM\config.inp" );
            mProjectM.setPresetCallback( OnPresetSwitched );
        }

        public void Handle( Events.ApplicationClosing args ) {
            StopVisualization();
        }

        public void StartVisualization() {
            mAudio.StartCapture( OnAudioData );

            mRenderTimer.Start();
        }

        public void StopVisualization() {
            mRenderTimer.Stop();

            mAudio.StopCapture();
        }

        public void OnSizeChanged( int width, int height ) {
            if( mProjectM.isInitialized()) {
                mProjectM.updateWindowSize( Scaler.Current.ScaleWidth( width ), Scaler.Current.ScaleHeight( height ));
            }
        }

        private void OnTimer( object sender, EventArgs args ) {
            mProjectM.renderFrame();

            mGlControl?.SwapBuffers();
        }

        private void OnAudioData( byte[] audioData, int samples, int channels ) {
            unsafe {
                fixed( byte * ptr = audioData ) {
                    mProjectM.addPCMfloat( ptr, samples, channels );
                }
            }
        }

        public void Dispose() {
            StopVisualization();

            mEventAggregator.Unsubscribe( this );

            mAudio?.Dispose();
            mGlControl?.Dispose();
        }
    }
}
