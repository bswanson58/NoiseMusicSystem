using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using HueLighting.Interfaces;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.Models.Groups;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.Streaming.Models;

namespace HueLighting.Models {
    class EntertainmentGroupManager : IEntertainmentGroupManager {
        private readonly IPreferences           mPreferences;
        private readonly IBasicLog              mLog;
        private StreamingHueClient              mStreamingClient;
        private StreamingGroup                  mStreamingGroup;
        private EntertainmentLayer              mBaseLayer;
        private CancellationTokenSource         mCancellationTokenSource;
        private double                          mBrightness;

        public  Group                           EntertainmentGroup { get; }

        public EntertainmentGroupManager( IPreferences preferences, IBasicLog log, Group forGroup ) {
            mLog = log;
            mPreferences = preferences;
            EntertainmentGroup = forGroup;
        }

        public async Task<bool> StartStreamingGroup() {
            bool    retValue;
            var     configuration = mPreferences.Load<HueConfiguration>();

            try {
                mStreamingClient = new StreamingHueClient( configuration.BridgeIp, configuration.BridgeAppKey, configuration.BridgeStreamingKey );
                mStreamingGroup = new StreamingGroup( EntertainmentGroup.Locations );
                mCancellationTokenSource = new CancellationTokenSource();

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

        public Task EnableAutoUpdate() {
            var retValue = Task.CompletedTask;

            if(( mStreamingClient != null ) &&
               ( mStreamingGroup != null )) {
                retValue = mStreamingClient.AutoUpdate( mStreamingGroup, mCancellationTokenSource.Token, 50, true );
            }

            return retValue;
        }

        public async Task<EntertainmentGroup> GetGroupLayout() {
            var retValue = default( EntertainmentGroup );

            try {
                var hubInfo = await mStreamingClient.LocalHueClient.GetBridgeAsync();
                
                retValue = new EntertainmentGroup( mBaseLayer, hubInfo?.Lights.ToList());
            }
            catch( Exception ex ) {
                mLog.LogException( "GetGroupLayout", ex );
            }

            return retValue;
        }

        public double OverallBrightness {
            get => mBrightness;
            set => mBrightness = Math.Min( 1.0, Math.Max( 0.0, value ));
        }

        public void SetLightColor( string lightId, Color toColor ) {
            var light = mBaseLayer?.FirstOrDefault( l => l.Id.ToString().Equals( lightId ));

            if( light != null ) {
                var color = new RGBColor( toColor.R, toColor.G, toColor.B );

                light.SetState( mCancellationTokenSource.Token, color, mBrightness );
            }
        }

        public void SetLightColor( string lightId, Color toColor, TimeSpan transitionTime ) {
            var light = mBaseLayer?.FirstOrDefault( l => l.Id.ToString().Equals( lightId ));

            if( light != null ) {
                var color = new RGBColor( toColor.R, toColor.G, toColor.B );

                light.SetState( mCancellationTokenSource.Token, color, transitionTime, mBrightness );
            }
        }

        public void UpdateLights() {
            mStreamingClient.ManualUpdate( mStreamingGroup, true );
        }

        public async Task<bool> IsStreamingActive() {
            var retValue = false;

            try {
                var bridgeInfo = await mStreamingClient.LocalHueClient.GetBridgeAsync();

                retValue = bridgeInfo?.IsStreamingActive == true;
            }
            catch( Exception ex ) {
                mLog.LogException( "IsStreamingActive", ex );
            }

            return retValue;
        }

        public void Dispose() {
            mCancellationTokenSource?.Cancel();
            mCancellationTokenSource = null;

            mStreamingClient?.Dispose();
            mStreamingClient = null;

            mStreamingGroup = null;
            mBaseLayer = null;
        }
    }
}
