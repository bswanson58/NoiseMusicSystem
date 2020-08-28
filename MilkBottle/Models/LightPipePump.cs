using System;
using System.Linq;
using Caliburn.Micro;
using HueLighting.Dto;
using HueLighting.Interfaces;
using LightPipe.Dto;
using LightPipe.Interfaces;
using LightPipe.Utility;
using MilkBottle.Dto;
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
        private bool                            mEnabled;
        private int                             mCaptureFrequency;
        private DateTime                        mLastCaptureTime;
        private IDisposable                     mZoneUpdateSubscription;

        public LightPipePump( IImageProcessor imageProcessor, IHubManager hubManager, IZoneManager zoneManager, IEventAggregator eventAggregator, IPreferences preferences ) {
            mImageProcessor = imageProcessor;
            mHubManager = hubManager;
            mZoneManager = zoneManager;
            mEventAggregator = eventAggregator;
            mPreferences = preferences;
        }

        public void Initialize() {
            var preferences = mPreferences.Load<MilkPreferences>();

            EnableLightPipe( preferences.LightPipeEnabled );
        }

        public async void EnableLightPipe( bool state ) {
            mEnabled = state;

            if( mEnabled ) {
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
        }

        public void SetCaptureFrequency( int milliseconds ) {
            mCaptureFrequency = milliseconds;
        }

        public void Handle( LightPipe.Events.FrameRendered args ) {
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

        public void Dispose() {
            EnableLightPipe( false );
        }
    }
}
