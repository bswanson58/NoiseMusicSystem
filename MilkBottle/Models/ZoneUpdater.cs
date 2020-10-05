using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using HueLighting.Interfaces;
using LightPipe.Dto;
using MilkBottle.Dto;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    internal class ZoneUpdater : IZoneUpdater {
        private readonly TimeSpan               mUpdateFrequency = TimeSpan.FromSeconds( 15 );

        private readonly IHubManager            mHubManager;
        private readonly IZoneManager           mZoneManager;
        private readonly IBasicLog              mLog;
        private readonly IPreferences           mPreferences;
        private readonly Subject<ZoneBulbState> mBulbStateSubject;
        private readonly List<BulbState>        mBulbStates;
        private IEntertainmentGroupManager      mEntertainmentGroupManager;
        private EntertainmentGroup              mEntertainmentGroup;
        private ZoneGroup                       mZoneGroup;
        private double                          mOverallLightBrightness;
        private Task                            mLightUpdateTask;
        private DateTime                        mLastCheckTime;

        public  int                             CaptureFrequency { get; set; }
        public  int                             ZoneColorsLimit { get; set; }

        public  bool                            IsRunning {get; private set; }

        public  IObservable<ZoneBulbState>      BulbStates => mBulbStateSubject;

        public ZoneUpdater( IHubManager hubManager, IZoneManager zoneManager, IBasicLog log, IPreferences preferences ) {
            mHubManager = hubManager;
            mZoneManager = zoneManager;
            mPreferences = preferences;
            mLog = log;

            mBulbStateSubject = new Subject<ZoneBulbState>();
            mBulbStates = new List<BulbState>();
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

        public async Task<bool> SetZoneBulbsInactive() {
            var retValue = false;

            if( mBulbStates.Any()) {
                try {
                    var preferences = mPreferences.Load<MilkPreferences>();

                    if( ColorConverter.ConvertFromString( preferences.InactiveBulbColor ) is Color inactiveColor ) {
                        await Task.Delay( 250 );
                        retValue = await mHubManager.SetBulbState( from b in mBulbStates select b.Bulb, inactiveColor, TimeSpan.FromSeconds( 3 ));

                        if( retValue ) {
                            mBulbStates.Clear();
                        }
                    }
                    else {
                        mLog.LogMessage( "MilkPreferences:InactiveBulbColor is not in the correct format (#ARBG)" );
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "Setting Inactive bulb state", ex );
                }
            }
            else {
                retValue = true;
            }

            return retValue;
        }

        public async Task<bool> Start() {
            try {
                mEntertainmentGroupManager = await mHubManager.StartEntertainmentGroup();

                if( mEntertainmentGroupManager != null ) {
                    mEntertainmentGroup = await mEntertainmentGroupManager.GetGroupLayout();
                    mLightUpdateTask = mEntertainmentGroupManager.EnableAutoUpdate();
                    mZoneGroup = mZoneManager.GetCurrentGroup();

                    mEntertainmentGroupManager.OverallBrightness = mOverallLightBrightness;
                    IsRunning = mEntertainmentGroup != null;
                    mLastCheckTime = DateTime.Now;
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

        public async Task<bool> InsureRunning() {
            if( mLastCheckTime + mUpdateFrequency < DateTime.Now ) {
                mLastCheckTime = DateTime.Now;

                if( mLightUpdateTask.Status != TaskStatus.Running ) {
                    mLightUpdateTask = mEntertainmentGroupManager.EnableAutoUpdate();

                    mLog.LogMessage( $"Light update task status is: {mLightUpdateTask.Status}" );
                }

                if(!await mEntertainmentGroupManager.IsStreamingActive()) {
                    Stop();

                    await Start();

                    mLog.LogMessage( "Entertainment group restarted." );
                }
            }

            return IsRunning;
        }

        public void UpdateZone( ZoneSummary zone ) {
            if(( IsRunning ) &&
               ( zone != null )) {
                try {
                    var stopWatch = Stopwatch.StartNew();
                    var zoneGroup = mZoneGroup?.Zones.FirstOrDefault( z => z.ZoneName.Equals( zone.ZoneId ));

                    if( zoneGroup != null ) {
                        var lightGroup = mEntertainmentGroup.GetLights( zoneGroup.LightLocation );

                        if( lightGroup != null ) {
                            var colors = zone.FindMeanColors( Math.Min( lightGroup.Lights.Count, ZoneColorsLimit ));

                            if( colors.Any()) {
                                var summary = new ZoneBulbState( zone.ZoneId );
                                var colorIndex = 0;

                                foreach( var light in lightGroup.Lights ) {
                                    if( CaptureFrequency > 100 ) {
                                        mEntertainmentGroupManager.SetLightColor( light.Id, colors[colorIndex], TimeSpan.FromMilliseconds( CaptureFrequency * 0.75 ));
                                    }
                                    else {
                                        mEntertainmentGroupManager.SetLightColor( light.Id, colors[colorIndex]);
                                    }

                                    summary.BulbStates.Add( new BulbState( light, colors[colorIndex]));
                                    UpdateBulbState( new BulbState( light, colors[colorIndex]));

                                    colorIndex++;
                                    if( colorIndex >= colors.Count ) {
                                        colorIndex = 0;
                                    }
                                }

                                summary.SetProcessingTime( stopWatch.ElapsedMilliseconds );
                                mBulbStateSubject.OnNext( summary );
                            }
                        }
                    }

                    stopWatch.Stop();
                }
                catch( Exception ex ) {
                    mLog.LogException( "UpdateZone", ex );

                    IsRunning = false;
                }
            }
        }

        private void UpdateBulbState( BulbState newState ) {
            var existing = mBulbStates.FirstOrDefault( b => b.Bulb.Name.Equals( newState.Bulb.Name ));

            if( existing != null ) {
                mBulbStates.Remove( existing );
            }

            mBulbStates.Add( newState );
        }
    }
}
