using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using HueLighting.Dto;
using HueLighting.Interfaces;
using LightPipe.Dto;
using LightPipe.Interfaces;
using LightPipe.Utility;
using MilkBottle.Dto;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class LightPipePump : ILightPipePump, IHandle<LightPipe.Events.FrameRendered> {
        private readonly IImageProcessor        mImageProcessor;
        private readonly IHubManager            mHubManager;
        private readonly IZoneManager           mZoneManager;
        private readonly IEventAggregator       mEventAggregator;
        private readonly IPreferences           mPreferences;
        private readonly IBasicLog              mLog;
        private IEntertainmentGroupManager      mEntertainmentGroupManager;
        private EntertainmentGroup              mEntertainmentGroup;
        private ZoneGroup                       mZoneGroup;
        private int                             mCaptureFrequency;
        private int                             mZoneColorsLimit;
        private DateTime                        mLastCaptureTime;
        private IDisposable                     mZoneUpdateSubscription;

        public  bool                            IsEnabled { get; private set; }

        public LightPipePump( IImageProcessor imageProcessor, IHubManager hubManager, IZoneManager zoneManager, IEventAggregator eventAggregator, IPreferences preferences, IBasicLog log ) {
            mImageProcessor = imageProcessor;
            mHubManager = hubManager;
            mZoneManager = zoneManager;
            mEventAggregator = eventAggregator;
            mPreferences = preferences;
            mLog = log;

            var milkPreferences = mPreferences.Load<MilkPreferences>();

            mCaptureFrequency = milkPreferences.LightPipeCaptureFrequency;
            mZoneColorsLimit = milkPreferences.ZoneColorsLimit;
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
            get {
                var retValue = 0.0;

                if( mEntertainmentGroupManager != null ) {
                    retValue = mEntertainmentGroupManager.OverallBrightness;
                }

                return retValue;
            } 
            set {
                if( mEntertainmentGroupManager != null ) {
                    mEntertainmentGroupManager.OverallBrightness = value;
                }
            }
        }

        public int CaptureFrequency {
            get => mCaptureFrequency;
            set {
                mCaptureFrequency = Math.Min( 1000, Math.Max( 30, value ));

                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.LightPipeCaptureFrequency = mCaptureFrequency;

                mPreferences.Save( preferences );
            }
        }

        public int ZoneColorsLimit {
            get => mZoneColorsLimit;
            set {
                mZoneColorsLimit = Math.Min( Math.Max( value, 1 ), 10 );

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

        private async Task<bool> SetLightPipeState( bool state ) {
            IsEnabled = state;

            if( IsEnabled ) {
                mEntertainmentGroupManager = await mHubManager.StartEntertainmentGroup();

                if( mEntertainmentGroupManager != null ) {
                    mEntertainmentGroup = await mEntertainmentGroupManager.GetGroupLayout();
                    mEntertainmentGroupManager.EnableAutoUpdate();
                    mZoneGroup = mZoneManager.GetCurrentGroup();

                    mZoneUpdateSubscription = mImageProcessor.ZoneUpdate.Subscribe( OnZoneUpdate );
                    mEventAggregator.Subscribe( this );
                }
                else {
                    IsEnabled = false;
                }
            }
            else {
                mEventAggregator.Unsubscribe( this );

                mZoneUpdateSubscription?.Dispose();
                mZoneUpdateSubscription = null;

                mEntertainmentGroupManager?.Dispose();
                mEntertainmentGroupManager = null;
                mEntertainmentGroup = null;
                mZoneGroup = null;
            }

            return IsEnabled;
        }

        private async void RestartHue() {
            if( IsEnabled ) {
                await SetLightPipeState( false );
                await SetLightPipeState( true );
            }
        }

        public void Handle( LightPipe.Events.FrameRendered args ) {
            if(( IsEnabled ) &&
               ( mLastCaptureTime + TimeSpan.FromMilliseconds( mCaptureFrequency ) < DateTime.Now )) {

                CaptureFrame( args.WindowPtr );

                mLastCaptureTime = DateTime.Now;
            }
        }

        private async void CaptureFrame( IntPtr hWnd ) {
            try {
                using( var bitmap = BitmapCapture.Capture( hWnd )) {
                    if( bitmap != null ) {
                        mImageProcessor.ProcessImage( bitmap );
                    }
                }

                var streamingActive = await mEntertainmentGroupManager.IsStreamingActive();

                if(!streamingActive ) {
                    RestartHue();
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "CaptureFrame", ex );
            }
        }

        private void OnZoneUpdate( ZoneSummary zone ) {
            var zoneGroup = mZoneGroup?.Zones.FirstOrDefault( z => z.ZoneName.Equals( zone.ZoneId ));

            if( zoneGroup != null ) {
                var lightGroup = mEntertainmentGroup.GetLights( zoneGroup.LightLocation );

                if( lightGroup != null ) {
                    var colors = zone.FindMeanColors( Math.Min( lightGroup.Lights.Count, mZoneColorsLimit ));

                    if( colors.Any()) {
                        var colorIndex = 0;

                        foreach( var light in lightGroup.Lights ) {
                            if( mCaptureFrequency > 100 ) {
                                mEntertainmentGroupManager.SetLightColor( light.Id, colors[colorIndex], TimeSpan.FromMilliseconds( mCaptureFrequency * 0.75 ));
                            }
                            else {
                                mEntertainmentGroupManager.SetLightColor( light.Id, colors[colorIndex]);
                            }

                            colorIndex++;
                            if( colorIndex >= colors.Count ) {
                                colorIndex = 0;
                            }
                        }
                    }
                }
            }
        }

        public async void Dispose() {
            await SetLightPipeState( false );
        }
    }
}
