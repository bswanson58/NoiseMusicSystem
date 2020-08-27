using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using HueLighting.Interfaces;
using JetBrains.Annotations;
using MilkBottle.Infrastructure.Interfaces;
using Q42.HueApi;

namespace HueLighting.Models {
    public class HubManager : IHubManager {
        private readonly IApplicationConstants  mApplicationConstants;
        private readonly IEnvironment           mEnvironment;
        private readonly IPreferences           mPreferences;
        private LocalHueClient                  mClient;
        private bool                            mEmulating;

        [UsedImplicitly]
        public HubManager( IEnvironment environment, IPreferences preferences, IApplicationConstants constants ) {
            mEnvironment = environment;
            mPreferences = preferences;
            mApplicationConstants = constants;

            mEmulating = false;
        }

        public async Task<bool> InitializeConfiguredHub() {
            var retValue = false;
            var installationInfo = mPreferences.Load<InstallationInfo>();

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
            var installationInfo = mPreferences.Load<InstallationInfo>();
            var bridges = await HueBridgeDiscovery.FastDiscoveryWithNetworkScanFallbackAsync( TimeSpan.FromSeconds( 5 ), TimeSpan.FromSeconds( 30 ));

            foreach( var bridge in bridges ) {
                try {
                    var client = new LocalHueClient( bridge.IpAddress , installationInfo.BridgeAppKey );

                    try {
                        var bridgeInfo = await client.GetBridgeAsync();

                        if( bridgeInfo?.Config != null ) {
                            retValue.Add( new HubInformation( bridge, bridgeInfo, installationInfo.BridgeAppKey, bridge.BridgeId.Equals( installationInfo.BridgeId )));
                        }
                    }
                    catch( Exception ) {
                        retValue.Add( new HubInformation( bridge ));
                    }

                }
                catch( Exception ) { }
            }

            return retValue;
        }

        public async Task<String> RegisterApp( HubInformation hub, bool setAsConfiguredHub ) {
            string retValue;

            try {
                var client = new LocalHueClient( hub.IpAddress );

                retValue = await client.RegisterAsync( mApplicationConstants.ApplicationName, mEnvironment.EnvironmentName());

                if( setAsConfiguredHub ) {
                    SetConfiguredHub( new HubInformation( hub, retValue ));
                }
            }
            catch( Exception ex ) {
                retValue = ex.Message;
            }

            return retValue;
        }

        public void SetConfiguredHub( HubInformation hub ) {
            var userPreferences = mPreferences.Load<InstallationInfo>();

            userPreferences.BridgeIp = hub.IpAddress;
            userPreferences.BridgeId = hub.BridgeId;
            userPreferences.BridgeAppKey = hub.BridgeAppKey;

            mPreferences.Save( userPreferences );
        }

        public async Task<IEnumerable<Bulb>> BulbList() {
            if( mClient != null ) {
                var lights = await mClient.GetLightsAsync();

                return from light in lights select new Bulb( light.Id, light.Name, light.State.IsReachable == true );
            }

            if( mEmulating ) {
                return BuildEmulationBulbSet();
            }

            return null;
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

        public async Task<bool> SetBulbState( String bulbId, bool state) {
            return await SetBulbState( new []{ bulbId}, state );
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

        public async Task<bool> SetBulbState( String bulbId, Color color ) {
            return await SetBulbState( new []{ bulbId}, color );
        }

        public async Task<bool> SetBulbState( IEnumerable<string> bulbList, Color color ) {
            if(( mClient == null ) ||
               ( mEmulating )) {
                return true;
            }

            var command = new LightCommand();

//            command.SetColor( new RGBColor( color.R, color.G, color.B ));
            var result = await mClient.SendCommandAsync( command, bulbList );

            return !result.HasErrors();
        }

        public async Task<bool> SetBulbState( string bulbId, int brightness ) {
            return await SetBulbState( new []{ bulbId }, brightness );
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
    }
}
