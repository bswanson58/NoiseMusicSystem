using System;
using System.Linq;
using HueLighting.Dto;
using HueLighting.Interfaces;
using LightPipe.Dto;
using LightPipe.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class LightPipePump : ILightPipePump {
        private readonly ILightPipeController   mLightPipe;
        private readonly IImageProcessor        mImageProcessor;
        private readonly IHubManager            mHubManager;
        private readonly IZoneManager           mZoneManager;
        private IEntertainmentGroupManager      mEntertainmentGroupManager;
        private EntertainmentGroup              mEntertainmentGroup;
        private ZoneGroup                       mZoneGroup;
        private IDisposable                     mZoneUpdateSubscription;

        public LightPipePump( ILightPipeController lightPipeController, IImageProcessor imageProcessor, IHubManager hubManager, IZoneManager zoneManager ) {
            mLightPipe = lightPipeController;
            mImageProcessor = imageProcessor;
            mHubManager = hubManager;
            mZoneManager = zoneManager;
        }

        public async void Initialize() {
            mLightPipe.Initialize();
            mEntertainmentGroupManager = await mHubManager.StartEntertainmentGroup();
            mEntertainmentGroup = await mEntertainmentGroupManager.GetGroupLayout();
            mZoneGroup = mZoneManager.GetCurrentGroup();

            mZoneUpdateSubscription = mImageProcessor.ZoneUpdate.Subscribe( OnZoneUpdate );
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
            mLightPipe?.Dispose();
            mZoneUpdateSubscription?.Dispose();
        }
    }
}
