using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using HueLighting.Interfaces;
using MilkBottle.Infrastructure.Interfaces;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.Models.Groups;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.Streaming.Models;

namespace HueLighting.Models {
    class EntertainmentGroupManager : IEntertainmentGroupManager {
        private readonly IPreferences           mPreferences;
        private StreamingHueClient              mStreamingClient;
        private StreamingGroup                  mStreamingGroup;
        private EntertainmentLayer              mBaseLayer;
        private CancellationTokenSource         mAutoUpdateToken;
        private double                          mBrightness;

        public  Group                           EntertainmentGroup { get; }

        public EntertainmentGroupManager( IPreferences preferences, Group forGroup ) {
            mPreferences = preferences;
            EntertainmentGroup = forGroup;

            var huePreferences = mPreferences.Load<HueConfiguration>();

            mBrightness = huePreferences.OverallBrightness;
        }

        public async Task<bool> StartStreamingGroup() {
            bool    retValue;
            var     configuration = mPreferences.Load<HueConfiguration>();

            try {
                mStreamingClient = new StreamingHueClient( configuration.BridgeIp, configuration.BridgeAppKey, configuration.BridgeStreamingKey );
                mStreamingGroup = new StreamingGroup( EntertainmentGroup.Locations );

                await mStreamingClient.Connect( EntertainmentGroup.Id );
                mBaseLayer = mStreamingGroup.GetNewLayer( true );

                var bridgeInfo = await mStreamingClient.LocalHueClient.GetBridgeAsync();

                retValue = mBaseLayer != null && bridgeInfo?.IsStreamingActive == true;
            }
            catch( Exception ) {
                retValue = false;
            }

            return retValue;
        }

        public void EnableAutoUpdate() {
            if(( mStreamingClient != null ) &&
               ( mStreamingGroup != null )) {
                mAutoUpdateToken = new CancellationTokenSource();

                mStreamingClient.AutoUpdate( mStreamingGroup, mAutoUpdateToken.Token, 50, true );
            }
        }

        public async Task<EntertainmentGroup> GetGroupLayout() {
            var hubInfo = await mStreamingClient.LocalHueClient.GetBridgeAsync();
                
            return new EntertainmentGroup( mBaseLayer, hubInfo?.Lights.ToList());
        }

        public double OverallBrightness {
            get => mBrightness;
            set {
                mBrightness = Math.Min( 1.0, Math.Max( 0.0, value ));

                var preferences = mPreferences.Load<HueConfiguration>();

                preferences.OverallBrightness = mBrightness;

                mPreferences.Save( preferences );
            }
        }

        public void SetLightColor( string lightId, Color toColor ) {
            var light = mBaseLayer?.FirstOrDefault( l => l.Id.ToString().Equals( lightId ));

            if( light != null ) {
                var color = new RGBColor( toColor.R, toColor.G, toColor.B );

                light.SetState( CancellationToken.None, color, mBrightness );
            }
        }

        public void SetLightColor( string lightId, Color toColor, TimeSpan transitionTime ) {
            var light = mBaseLayer?.FirstOrDefault( l => l.Id.ToString().Equals( lightId ));

            if( light != null ) {
                var color = new RGBColor( toColor.R, toColor.G, toColor.B );

                light.SetState( CancellationToken.None, color, transitionTime, mBrightness );
            }
        }

        public void UpdateLights() {
            mStreamingClient.ManualUpdate( mStreamingGroup, true );
        }

        public void Dispose() {
            mAutoUpdateToken?.Cancel();
            mAutoUpdateToken = null;

            mStreamingClient?.Dispose();
            mStreamingClient = null;

            mStreamingGroup = null;
            mBaseLayer = null;
        }
    }
}
