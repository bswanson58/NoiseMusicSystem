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
        private IEntertainmentGroupManager      mEntertainmentGroupManager;
        private EntertainmentGroup              mEntertainmentGroup;
        private ZoneGroup                       mZoneGroup;
        private int                             mCaptureFrequency;
        private DateTime                        mLastCaptureTime;
        private IDisposable                     mZoneUpdateSubscription;

        public  bool                            IsEnabled { get; private set; }

        public LightPipePump( IImageProcessor imageProcessor, IHubManager hubManager, IZoneManager zoneManager, IEventAggregator eventAggregator, IPreferences preferences ) {
            mImageProcessor = imageProcessor;
            mHubManager = hubManager;
            mZoneManager = zoneManager;
            mEventAggregator = eventAggregator;
            mPreferences = preferences;
        }

        public async Task<bool> EnableLightPipe( bool state, bool startLightPipeIfDesired ) {
            var preferences = mPreferences.Load<MilkPreferences>();

            preferences.LightPipeEnabled = state;

            mPreferences.Save( preferences );

            if(!state ) {
                await SetLightPipeState( false );
            }
            else if( startLightPipeIfDesired ) {
                await SetLightPipeState( true );
            }

            return IsEnabled;
        }

        public async Task<bool> Initialize() {
            var preferences = mPreferences.Load<MilkPreferences>();

            return await SetLightPipeState( preferences.LightPipeEnabled );
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

        private async Task<bool> SetLightPipeState( bool state ) {
            IsEnabled = state;

            if( IsEnabled ) {
                mEntertainmentGroupManager = await mHubManager.StartEntertainmentGroup();
                mEntertainmentGroup = await mEntertainmentGroupManager.GetGroupLayout();
                mZoneGroup = mZoneManager.GetCurrentGroup();

                mZoneUpdateSubscription = mImageProcessor.ZoneUpdate.Subscribe( OnZoneUpdate );
                mEventAggregator.Subscribe( this );
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

        public void SetCaptureFrequency( int milliseconds ) {
            mCaptureFrequency = milliseconds;
        }

        public void Handle( LightPipe.Events.FrameRendered args ) {
            if(( IsEnabled ) &&
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

        private void OnZoneUpdate( ZoneSummary zone ) {
            var zoneGroup = mZoneGroup.Zones.FirstOrDefault( z => z.ZoneName.Equals( zone.ZoneId ));

            if( zoneGroup != null ) {
                var lightGroup = mEntertainmentGroup.GetLights( zoneGroup.LightLocation );

                if( lightGroup != null ) {
                    var colors = zone.FindMeanColors( lightGroup.Lights.Count );

                    if( colors.Any()) {
                        var colorIndex = 0;

                        foreach( var light in lightGroup.Lights ) {
                            mEntertainmentGroupManager.SetLightColor( light.Id, colors[colorIndex]);

                            colorIndex++;
                            if( colorIndex >= colors.Count ) {
                                colorIndex = 0;
                            }
                        }

                        mEntertainmentGroupManager.UpdateLights();
                    }
                }
            }
        }

        public async void Dispose() {
            await SetLightPipeState( false );
        }
    }
}
