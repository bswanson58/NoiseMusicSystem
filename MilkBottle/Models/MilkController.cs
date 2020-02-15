using System;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Interfaces;
using MilkBottle.Support;
using OpenTK;
using WindowState = System.Windows.WindowState;

namespace MilkBottle.Models {
    class MilkController : IMilkController, IHandle<Events.ApplicationClosing>, IHandle<Events.WindowStateChanged> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IEnvironment               mEnvironment;
        private readonly DispatcherTimer            mRenderTimer;
        private readonly ProjectMWrapper            mProjectM;
        private readonly IAudioManager              mAudio;
        private GLControl                           mGlControl;

        public  bool                                IsRunning { get; private set; }

        public MilkController( ProjectMWrapper projectM, IAudioManager audioManager, IEnvironment environment, IEventAggregator eventAggregator ) {
            mAudio = audioManager;
            mEventAggregator = eventAggregator;
            mEnvironment = environment;
            mProjectM = projectM;

            mRenderTimer = new DispatcherTimer( DispatcherPriority.Normal ) { Interval = TimeSpan.FromMilliseconds( 34 ) };
            mRenderTimer.Tick += OnTimer;
            IsRunning = false;

            mEventAggregator.Subscribe( this );
        }

        public void Initialize( GLControl glControl ) {
            mGlControl = glControl;
            mGlControl.MakeCurrent();

            mProjectM.initialize( mEnvironment.MilkConfigurationFile());
            mAudio.InitializeAudioCapture();

            mEventAggregator.PublishOnUIThread( new Events.MilkInitialized());
        }

        public void Handle( Events.ApplicationClosing args ) {
            StopVisualization();
        }

        public void Handle( Events.WindowStateChanged args ) {
            if((!IsRunning ) &&
               ( args.CurrentState != WindowState.Minimized )) {
                UpdateVisualization();
            }
        }

        public void StartVisualization() {
            mAudio.StartCapture( OnAudioData );
            mRenderTimer.Start();

            IsRunning = true;
        }

        public void StopVisualization() {
            mRenderTimer.Stop();
            mAudio.StopCapture();

            IsRunning = false;
        }

        public void OnSizeChanged( int width, int height ) {
            if( mProjectM.isInitialized()) {
                mProjectM.updateWindowSize( Scaler.Current.ScaleWidth( width ), Scaler.Current.ScaleHeight( height ));
            }
        }

        private void OnTimer( object sender, EventArgs args ) {
            UpdateVisualization();
        }

        private void UpdateVisualization() {
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
