﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ColorMine.ColorSpaces;
using HueLighting.Dto;
using HueLighting.Interfaces;
using JetBrains.Annotations;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Groups;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Models;

namespace HueLighting.Models {
    public class HubManager : IHubManager {
        private readonly IApplicationConstants  mApplicationConstants;
        private readonly IEnvironment           mEnvironment;
        private readonly IPreferences           mPreferences;
        private readonly IBasicLog              mLog;
        private ILocalHueClient                 mClient;
        private bool                            mEmulating;

        public  bool                            IsInitialized => mClient != null;

        [UsedImplicitly]
        public HubManager( IEnvironment environment, IPreferences preferences, IBasicLog log, IApplicationConstants constants ) {
            mEnvironment = environment;
            mPreferences = preferences;
            mLog = log;
            mApplicationConstants = constants;

            mEmulating = false;
        }

        public async Task<bool> InitializeConfiguredHub() {
            var retValue = false;
            var installationInfo = mPreferences.Load<HueConfiguration>();

            if(!String.IsNullOrWhiteSpace( installationInfo.BridgeIp )) {
                try {
                    mClient = new LocalHueClient( installationInfo.BridgeIp, installationInfo.BridgeAppKey );

                    retValue = await mClient.CheckConnection();

                    if(!retValue ) {
                        mClient = null;
                    }
                }
                catch( Exception ) {
                    mClient = null;

                    retValue = false;
                }
            }

            return retValue;
        }

        public void EmulateHub() {
            mEmulating = true;
        }

        public async Task<IEnumerable<HubInformation>> LocateHubs() { 
            var retValue = new List<HubInformation>();
            var installationInfo = mPreferences.Load<HueConfiguration>();
            var bridges = await HueBridgeDiscovery.FastDiscoveryWithNetworkScanFallbackAsync( TimeSpan.FromSeconds( 5 ), TimeSpan.FromSeconds( 30 ));

            foreach( var bridge in bridges ) {
                try {
                    var client = new LocalHueClient( bridge.IpAddress , installationInfo.BridgeAppKey, installationInfo.BridgeStreamingKey );

                    try {
                        var bridgeInfo = await client.GetBridgeAsync();

                        if( bridgeInfo?.Config != null ) {
                            retValue.Add( new HubInformation( bridge, bridgeInfo, installationInfo.BridgeAppKey, installationInfo.BridgeStreamingKey,
                                                              bridge.BridgeId.Equals( installationInfo.BridgeId )));
                        }
                    }
                    catch( Exception ) {
                        retValue.Add( new HubInformation( bridge ));
                    }

                }
                catch( Exception ex ) {
                    mLog.LogException( "LocateHubs", ex );
                }
            }

            return retValue;
        }

        public async Task<String> RegisterApp( HubInformation hub, bool setAsConfiguredHub ) {
            var retValue = String.Empty;

            try {
                var client = new LocalHueClient( hub.IpAddress );
                var clientKey = await client.RegisterAsync( mApplicationConstants.ApplicationName, mEnvironment.EnvironmentName(), true );

                if( setAsConfiguredHub ) {
                    SetConfiguredHub( new HubInformation( hub, clientKey?.Username, clientKey?.StreamingClientKey ));
                }
            }
            catch( Exception ex ) {
                retValue = ex.Message;
            }

            return retValue;
        }

        public void SetConfiguredHub( HubInformation hub ) {
            var userPreferences = mPreferences.Load<HueConfiguration>();

            userPreferences.BridgeIp = hub.IpAddress;
            userPreferences.BridgeId = hub.BridgeId;
            userPreferences.BridgeAppKey = hub.BridgeAppKey;
            userPreferences.BridgeStreamingKey = hub.StreamingKey;

            mPreferences.Save( userPreferences );
        }

        public async Task<IEnumerable<Bulb>> GetBulbs() {
            var retValue = default( IEnumerable<Bulb>);

            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    var lights = await mClient.GetLightsAsync();

                    retValue = from light in lights select new Bulb( light.Id, light.Name, light.State.IsReachable == true );
                }

                if( mEmulating ) {
                    return BuildEmulationBulbSet();
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "GetBulbs", ex );
            }

            return retValue;
        }

        public async Task<IEnumerable<BulbGroup>> GetBulbGroups() {
            var retValue = default( IEnumerable<BulbGroup>);

            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    retValue = await ToBulbGroup( await mClient.GetGroupsAsync());
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "GetBulbGroups", ex );
            }

            return retValue;
        }

        private async Task<IEnumerable<BulbGroup>> ToBulbGroup( IEnumerable<Group> fromList ) {
            var retValue = new List<BulbGroup>();

            if( fromList != null ) {
                var bulbList = await GetBulbs();

                foreach( var g in fromList ) {
                    retValue.Add( new BulbGroup( g.Name, g.Type, from bulb in g.Lights select bulbList.FirstOrDefault( b => b.Id.Equals( bulb ))));
                }
            }

            return retValue;
        }

        public async Task<IEnumerable<Group>> GetEntertainmentGroups() {
            var retValue = default( IEnumerable<Group>);

            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    retValue = await mClient.GetEntertainmentGroups();
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "GetEntertainmentGroups", ex );
            }

            return retValue;
        }

        public async Task<EntertainmentGroup> GetEntertainmentGroupLayout( Group forGroup ) {
            var retValue = default( EntertainmentGroup );

            try {
                var configuration = mPreferences.Load<HueConfiguration>();
                using( var streamingClient = new StreamingHueClient( configuration.BridgeIp, configuration.BridgeAppKey, configuration.BridgeStreamingKey )) {
                    await streamingClient.Connect( forGroup.Id );

                    var streamingGroup = new StreamingGroup( forGroup.Locations );
                    var baseLayer = streamingGroup.GetNewLayer( true );
                    var hubInfo = await mClient.GetBridgeAsync();
                
                    retValue = new EntertainmentGroup( baseLayer, hubInfo?.Lights.ToList());
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "GetEntertainmentGroupLayout", ex );
            }

            return retValue;
        }

        public async Task<IEntertainmentGroupManager> StartEntertainmentGroup() {
            var retValue = default( IEntertainmentGroupManager );

            try {
                var preferences = mPreferences.Load<HueConfiguration>();
                var groups = await GetEntertainmentGroups();
                var group = groups.FirstOrDefault( g => g.Id.Equals( preferences.EntertainmentGroupId ));

                if( group != null ) {
                    retValue = await StartEntertainmentGroup( group );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "StartEntertainmentGroup", ex );
            }

            return retValue;
        }

        public async Task<IEntertainmentGroupManager> StartEntertainmentGroup( Group forGroup ) {
            var retValue = new EntertainmentGroupManager( mPreferences, mLog, forGroup );

            await retValue.StartStreamingGroup();

            return retValue;
        }

        private IEnumerable<Bulb> BuildEmulationBulbSet() {
            var retValue = new List<Bulb> {
                                new Bulb( "1", "Illuminate 1", true ),
                                new Bulb( "2", "Illuminate 2", true ),
                                new Bulb( "3", "Illuminate 3", true ),
                                new Bulb( "4", "Illuminate 4", true ),
                                new Bulb( "5", "Illuminate 5", true )
                            };

            return retValue;
        }

        public async Task<bool> SetBulbState( Bulb bulb, bool state) {
            return await SetBulbState( new []{ bulb.Id }, state );
        }

        public async Task<bool> SetBulbState( IEnumerable<Bulb> bulbList, bool state ) {
            return await SetBulbState( from b in bulbList select b.Id, state );
        }
         
        public async Task<bool> SetBulbState( IEnumerable<string> bulbList, bool state ) {
            if(( mClient == null ) ||
               ( mEmulating )) {
                return true;
            }

            var command = new LightCommand{ On = state };
            var result = await mClient.SendCommandAsync( command, bulbList );

            return !result.HasErrors();
        }

        public async Task<bool> SetBulbState( Bulb bulb, int brightness ) {
            return await SetBulbState( new []{ bulb.Id }, brightness );
        }

        public async Task<bool> SetBulbState( IEnumerable<Bulb> bulbList, int brightness ) {
            return await SetBulbState( from b in bulbList select b.Id, brightness );
        }

        public async Task<bool> SetBulbState( IEnumerable<string> bulbList, int brightness ) {
            if(( mClient == null ) ||
               ( mEmulating )) {
                return true;
            }

            brightness = Math.Max( Math.Min( 255, brightness ), 0 );

            var command = new LightCommand{ Brightness = (byte)brightness };
            var result = await mClient.SendCommandAsync( command, bulbList );

            return !result.HasErrors();
        }

        public async Task<bool> SetBulbState( Bulb bulb, Color color, TimeSpan? transitionTime = null ) {
            return await SetBulbState( new []{ bulb.Id }, color, transitionTime );
        }

        public async Task<bool> SetBulbState( IEnumerable<Bulb> bulbList, Color color, TimeSpan? transitionTime = null ) {
            return await SetBulbState( from b in bulbList select b.Id, color, transitionTime );
        }

        public async Task<bool> SetBulbState( IEnumerable<string> bulbList, Color color, TimeSpan? transitionTime = null ) {
            if(( mClient == null ) ||
               ( mEmulating )) {
                return true;
            }

            var command = new LightCommand();
            var hsbColor = new Rgb { R = color.R, G = color.G, B = color.B }.To<Hsb>();

            command.Hue = Math.Max( Math.Min( 65535, (int)(( hsbColor.H / 360.0 ) * 65535 )), 0 );
            command.Saturation = Math.Max( Math.Min( 254, (int)( hsbColor.S * 254 )), 0 );
            command.Brightness = Math.Max( Math.Min( (byte)254, (byte)( hsbColor.B * 254 )), (byte)1 );
            command.TransitionTime = transitionTime;

            var result = await mClient.SendCommandAsync( command, bulbList );

            return !result.HasErrors();
        }
    }
}
