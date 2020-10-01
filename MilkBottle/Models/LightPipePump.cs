using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using LightPipe.Dto;
using LightPipe.Interfaces;
using LightPipe.Utility;
using MilkBottle.Dto;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    internal class LightPipePump : ILightPipePump, IHandle<LightPipe.Events.FrameRendered> {
        private readonly IImageProcessor            mImageProcessor;
        private readonly IZoneUpdater               mZoneUpdater;
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPreferences               mPreferences;
        private readonly IBasicLog                  mLog;
        private CancellationTokenSource             mCancellationTokenSource;
        private BlockingCollection<Bitmap>          mCaptureQueue;
        private BlockingCollection<ZoneSummary>     mSummaryQueue;
        private Task                                mCaptureTask;
        private Task                                mSummaryTask;
        private int                                 mCaptureFrequency;
        private int                                 mZoneColorsLimit;
        private DateTime                            mLastCaptureTime;
        private IDisposable                         mZoneUpdateSubscription;

        public  bool                                IsEnabled { get; private set; }

        public LightPipePump( IImageProcessor imageProcessor, IZoneUpdater zoneUpdater,
                              IEventAggregator eventAggregator, IPreferences preferences, IBasicLog log ) {
            mImageProcessor = imageProcessor;
            mZoneUpdater = zoneUpdater;
            mEventAggregator = eventAggregator;
            mPreferences = preferences;
            mLog = log;

            var milkPreferences = mPreferences.Load<MilkPreferences>();

            CaptureFrequency = milkPreferences.LightPipeCaptureFrequency;
            ZoneColorsLimit = milkPreferences.ZoneColorsLimit;
            OverallBrightness = milkPreferences.OverallBrightness;
        }

        public async Task<bool> EnableLightPipe( bool state, bool startLightPipeIfDesired ) {
            if(!state ) {
                await SetLightPipeState( false );
            }
            else if( startLightPipeIfDesired ) {
                await SetLightPipeState( true );
            }

            return IsEnabled;
        }

        public async Task<bool> Initialize() {
            return await SetLightPipeState( false );
        }

        public double OverallBrightness {
            get => mZoneUpdater.OverallLightBrightness;
            set {
                mZoneUpdater.OverallLightBrightness = value;

                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.OverallBrightness = value;

                mPreferences.Save( preferences );
            }
        }

        public int CaptureFrequency {
            get => mCaptureFrequency;
            set {
                mCaptureFrequency = Math.Min( 1000, Math.Max( 30, value ));
                mZoneUpdater.CaptureFrequency = mCaptureFrequency;

                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.LightPipeCaptureFrequency = mCaptureFrequency;

                mPreferences.Save( preferences );
            }
        }

        public int ZoneColorsLimit {
            get => mZoneColorsLimit;
            set {
                mZoneColorsLimit = Math.Min( Math.Max( value, 1 ), 10 );
                mZoneUpdater.ZoneColorsLimit = mZoneColorsLimit;

                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.ZoneColorsLimit = mZoneColorsLimit;

                mPreferences.Save( preferences );
            }
        }

        public int WhitenessLimit {
            get => mImageProcessor.WhitenessLimit;
            set => mImageProcessor.WhitenessLimit = value;
        }

        public int BlacknessLimit {
            get => mImageProcessor.BlacknessLimit;
            set => mImageProcessor.BlacknessLimit = value;
        }

        public bool BoostLuminosity {
            get => mImageProcessor.BoostLuminosity;
            set => mImageProcessor.BoostLuminosity = value;
        }

        public bool BoostSaturation {
            get => mImageProcessor.BoostSaturation;
            set => mImageProcessor.BoostSaturation = value;
        }

        private async Task<bool> SetLightPipeState( bool state ) {
            IsEnabled = state;

            if( IsEnabled ) {
                try {
                    if( await mZoneUpdater.Start()) {
                        mCaptureQueue = new BlockingCollection<Bitmap>( 10 );
                        mSummaryQueue = new BlockingCollection<ZoneSummary>( 30 );

                        mCancellationTokenSource = new CancellationTokenSource();
                        mCaptureTask = Task.Run( () => CaptureConsumer( mCancellationTokenSource.Token ), mCancellationTokenSource.Token );
                        mSummaryTask = Task.Run( () => SummaryConsumer( mCancellationTokenSource.Token ), mCancellationTokenSource.Token );

                        mZoneUpdateSubscription = mImageProcessor.ZoneUpdate.Subscribe( OnZoneUpdate );
                        mEventAggregator.Subscribe( this );
                    }
                    else {
                        IsEnabled = false;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "SetLightPipeState:true", ex );

                    IsEnabled = false;
                }
            }
            else {
                mCaptureQueue?.CompleteAdding();
                mSummaryQueue?.CompleteAdding();
                mCancellationTokenSource?.Cancel();

                mZoneUpdater.Stop();
                mEventAggregator.Unsubscribe( this );

                mCaptureTask?.Wait( 200 );
                mSummaryTask?.Wait( 200 );
                mCaptureTask = null;
                mSummaryTask = null;

                mZoneUpdateSubscription?.Dispose();
                mZoneUpdateSubscription = null;
            }

            return IsEnabled;
        }

        public void Handle( LightPipe.Events.FrameRendered args ) {
            if(( IsEnabled ) &&
               ( mLastCaptureTime + TimeSpan.FromMilliseconds( mCaptureFrequency ) < DateTime.Now )) {

                CaptureFrame( args.WindowPtr );

                mLastCaptureTime = DateTime.Now;
            }
        }

        private void CaptureFrame( IntPtr hWnd ) {
            try {
                if(!mCaptureQueue.IsAddingCompleted ) {
                    mCaptureQueue.TryAdd( BitmapCapture.Capture( hWnd ));
                }
            }
            catch( OperationCanceledException ) { }
            catch( Exception ex ) {
                mLog.LogException( "CaptureFrame", ex );
            }
        }

        private void CaptureConsumer( CancellationToken cancelToken ) {
            while(!mCaptureQueue.IsCompleted ) {
                try {
                    using( var image = mCaptureQueue.Take( cancelToken )) {
                        if( cancelToken.IsCancellationRequested ) {
                            break;
                        }

                        if( image != null ) {
                            mImageProcessor.ProcessImage( image );
                        }
                    }
                }
                catch( OperationCanceledException ) {
                    break;
                }
            }
        }

        private void OnZoneUpdate( ZoneSummary zone ) {
            try {
                if(( zone != null ) &&
                   (!mSummaryQueue.IsAddingCompleted )) {
                    mSummaryQueue.TryAdd( zone );
                }
            }
            catch( OperationCanceledException ) { }
        }

        private async void SummaryConsumer( CancellationToken cancelToken ) {
            while(!mSummaryQueue.IsCompleted ) {
                try {
                    var summary = mSummaryQueue.Take( cancelToken );

                    if( cancelToken.IsCancellationRequested ) {
                        break;
                    }

                    mZoneUpdater.UpdateZone( summary );

                    if(!await mZoneUpdater.CheckRunning()) {
                        if( IsEnabled ) {
                            await mZoneUpdater.Start();
                        }
                    }
                }
                catch( OperationCanceledException ) {
                    break;
                }
            }
        }

        public async void Dispose() {
            await SetLightPipeState( false );
        }
    }
}
