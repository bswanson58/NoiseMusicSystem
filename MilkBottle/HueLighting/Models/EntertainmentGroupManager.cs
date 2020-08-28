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

        public  Group                           EntertainmentGroup { get; }

        public EntertainmentGroupManager( IPreferences preferences, Group forGroup ) {
            mPreferences = preferences;
            EntertainmentGroup = forGroup;
        }

        public async Task<bool> StartStreamingGroup() {
            bool    retValue;
            var     configuration = mPreferences.Load<HueConfiguration>();

            try {
                mStreamingClient = new StreamingHueClient( configuration.BridgeIp, configuration.BridgeAppKey, configuration.BridgeStreamingKey );
                mStreamingGroup = new StreamingGroup( EntertainmentGroup.Locations );

                await mStreamingClient.Connect( EntertainmentGroup.Id );
                mBaseLayer = mStreamingGroup.GetNewLayer( true );

                mStreamingClient.AutoUpdate( mStreamingGroup, CancellationToken.None );

                retValue = mBaseLayer != null;
            }
            catch( Exception ) {
                retValue = false;
            }

            return retValue;
        }

        public async Task<EntertainmentGroup> GetGroupLayout() {
            var hubInfo = await mStreamingClient.LocalHueClient.GetBridgeAsync();
                
            return new EntertainmentGroup( mBaseLayer, hubInfo?.Lights.ToList());
        }

        public void SetLightColor( string lightId, Color toColor ) {
            var light = mBaseLayer.FirstOrDefault( l => l.Id.ToString().Equals( lightId ));

            if( light != null ) {
                var color = new RGBColor( toColor.R, toColor.G, toColor.B );

                if( light.State.Brightness < 0.1 ) {
                    light.SetBrightness( CancellationToken.None, 1 );
                }

                light.SetColor( CancellationToken.None, color );
            }
        }
    }
}
