using System;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Support;
using OpenTK;
using WindowState = System.Windows.WindowState;

namespace MilkBottle.Models {
    class MilkController : IMilkController, 
                           IHandle<Events.WindowStateChanged> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IEnvironment               mEnvironment;
        private readonly IPreferences               mPreferences;
        private readonly DispatcherTimer            mRenderTimer;
        private readonly ProjectMWrapper            mProjectM;
        private readonly IAudioManager              mAudio;
        private GLControl                           mGlControl;
        private Size                                mLastWindowSize;

        public  bool                                IsRunning { get; private set; }

        public MilkController( ProjectMWrapper projectM, IAudioManager audioManager, IPreferences preferences, IEnvironment environment, IEventAggregator eventAggregator ) {
            mAudio = audioManager;
            mEventAggregator = eventAggregator;
            mPreferences = preferences;
            mEnvironment = environment;
            mProjectM = projectM;

            mLastWindowSize = Size.Empty;
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
        }

        public void MilkConfigurationUpdated() {
            var wasRunning = IsRunning;

            if( wasRunning ) {
                StopVisualization();
            }

            InitializeMilk();

            if( wasRunning ) {
                StartVisualization();
            }
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
            UpdateWindowSize( mLastWindowSize );
//            mProjectM.showFrameRate( true );

            // the timer gets a little boost to compensate for it's dispatcher priority.
            mRenderTimer.Interval = TimeSpan.FromTicks(((int)( TimeSpan.TicksPerSecond * 0.8 )) / settings.FrameRate );
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

        public void OnSizeChanged( Size newSize ) {
            mLastWindowSize = newSize;

            UpdateWindowSize( mLastWindowSize );
        }

        private void UpdateWindowSize( Size size ) {
            if(( mProjectM.isInitialized()) &&
               (!size.IsEmpty )) {
                mProjectM.updateWindowSize( Scaler.Current.ScaleWidth((int)size.Width ), Scaler.Current.ScaleHeight((int)size.Height ));
            }
        }

        private void OnTimer( object sender, EventArgs args ) {
            UpdateVisualization();
        }

        private void UpdateVisualization() {
            mProjectM.renderFrame();

            if( mGlControl != null ) {
                mGlControl.SwapBuffers();

                mEventAggregator.PublishOnUIThread( new LightPipe.Events.FrameRendered( mGlControl.WindowInfo.Handle ));
            }
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
