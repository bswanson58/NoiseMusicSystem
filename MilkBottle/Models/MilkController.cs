using System;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Support;
using OpenTK;
using WindowState = System.Windows.WindowState;

namespace MilkBottle.Models {
    class MilkController : IMilkController, 
                           IHandle<Events.ApplicationClosing>, IHandle<Events.WindowStateChanged>, IHandle<Events.MilkConfigurationUpdated> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IEnvironment               mEnvironment;
        private readonly IPreferences               mPreferences;
        private readonly DispatcherTimer            mRenderTimer;
        private readonly ProjectMWrapper            mProjectM;
        private readonly IAudioManager              mAudio;
        private GLControl                           mGlControl;

        public  bool                                IsRunning { get; private set; }

        public MilkController( ProjectMWrapper projectM, IAudioManager audioManager, IPreferences preferences, IEnvironment environment, IEventAggregator eventAggregator ) {
            mAudio = audioManager;
            mEventAggregator = eventAggregator;
            mPreferences = preferences;
            mEnvironment = environment;
            mProjectM = projectM;

            mRenderTimer = new DispatcherTimer( DispatcherPriority.Send );
            mRenderTimer.Tick += OnTimer;
            IsRunning = false;

            mEventAggregator.Subscribe( this );
        }

        public void Initialize( GLControl glControl ) {
            mGlControl = glControl;
            mGlControl.MakeCurrent();

            InitializeMilk();
            mAudio.InitializeAudioCapture();

            mEventAggregator.PublishOnUIThread( new Events.MilkInitialized());
        }

        private void InitializeMilk() {
            var settings = mPreferences.Load<MilkConfiguration>();
            var nativeSettings = new ProjectMSettings();

            settings.SetNativeConfiguration( nativeSettings );
            nativeSettings.DataFolder = mEnvironment.MilkTextureFolder();
            nativeSettings.PresetFolder = mEnvironment.MilkTextureFolder();

            if( mProjectM.isInitialized()) {
                nativeSettings.WindowHeight = mProjectM.getWindowHeight();
                nativeSettings.WindowWidth = mProjectM.getWindowWidth();
            }

            mProjectM.initialize( nativeSettings );
//            mProjectM.showFrameRate( true );

            // the timer gets a little boost to compensate for it's dispatcher priority.
            mRenderTimer.Interval = TimeSpan.FromTicks(((int)( TimeSpan.TicksPerSecond * 0.8 )) / settings.FrameRate );
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

        public void Handle( Events.MilkConfigurationUpdated args ) {
            var wasRunning = IsRunning;

            if( wasRunning ) {
                StopVisualization();
            }

            InitializeMilk();

            if( wasRunning ) {
                StartVisualization();
            }

            mEventAggregator.PublishOnUIThread( new Events.MilkUpdated());
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
