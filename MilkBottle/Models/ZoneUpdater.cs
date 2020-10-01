using System;
using System.Linq;
using System.Threading.Tasks;
using HueLighting.Dto;
using HueLighting.Interfaces;
using LightPipe.Dto;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    internal class ZoneUpdater : IZoneUpdater {
        private readonly IHubManager        mHubManager;
        private readonly IZoneManager       mZoneManager;
        private readonly IBasicLog          mLog;
        private IEntertainmentGroupManager  mEntertainmentGroupManager;
        private EntertainmentGroup          mEntertainmentGroup;
        private ZoneGroup                   mZoneGroup;
        private double                      mOverallLightBrightness;

        public  int                         CaptureFrequency { get; set; }
        public  int                         ZoneColorsLimit { get; set; }

        public  bool                        IsRunning {get; private set; }

        public ZoneUpdater( IHubManager hubManager, IZoneManager zoneManager, IBasicLog log ) {
            mHubManager = hubManager;
            mZoneManager = zoneManager;
            mLog = log;
        }

        public double OverallLightBrightness {
            get => mOverallLightBrightness;
            set {
                mOverallLightBrightness = value;

                if( mEntertainmentGroupManager != null ) {
                    mEntertainmentGroupManager.OverallBrightness = mOverallLightBrightness;
                }
            }
        }

        public async Task<bool> Start() {
            try {
                mEntertainmentGroupManager = await mHubManager.StartEntertainmentGroup();

                if( mEntertainmentGroupManager != null ) {
                    mEntertainmentGroup = await mEntertainmentGroupManager.GetGroupLayout();
                    mEntertainmentGroupManager.EnableAutoUpdate();
                    mZoneGroup = mZoneManager.GetCurrentGroup();

                    mEntertainmentGroupManager.OverallBrightness = mOverallLightBrightness;
                    IsRunning = mEntertainmentGroup != null;
                }
                else {
                    IsRunning = false;
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "SetLightPipeState:true", ex );

                IsRunning = false;
            }

            return IsRunning;
        }

        public void Stop() {
            IsRunning = false;

            mEntertainmentGroupManager?.Dispose();
            mEntertainmentGroupManager = null;
            mEntertainmentGroup = null;
            mZoneGroup = null;
        }

        public async Task<bool> CheckRunning() {
            return IsRunning && await mEntertainmentGroupManager.IsStreamingActive();
        }

        public void UpdateZone( ZoneSummary zone ) {
            if(( IsRunning ) &&
               ( zone != null )) {
                try {
                    var zoneGroup = mZoneGroup?.Zones.FirstOrDefault( z => z.ZoneName.Equals( zone.ZoneId ));

                    if( zoneGroup != null ) {
                        var lightGroup = mEntertainmentGroup.GetLights( zoneGroup.LightLocation );

                        if( lightGroup != null ) {
                            var colors = zone.FindMeanColors( Math.Min( lightGroup.Lights.Count, ZoneColorsLimit ));

                            if( colors.Any()) {
                                var colorIndex = 0;

                                foreach( var light in lightGroup.Lights ) {
                                    if( CaptureFrequency > 100 ) {
                                        mEntertainmentGroupManager.SetLightColor( light.Id, colors[colorIndex], TimeSpan.FromMilliseconds( CaptureFrequency * 0.75 ));
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
                catch( Exception ex ) {
                    mLog.LogException( "UpdateZone", ex );

                    IsRunning = false;
                }
            }
        }
    }
}
