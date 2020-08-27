using System;
using System.Threading.Tasks;
using HueLighting.Dto;
using HueLighting.Interfaces;
using MilkBottle.Infrastructure.Interfaces;
using Q42.HueApi.Models.Groups;
using Q42.HueApi.Streaming;
using Q42.HueApi.Streaming.Models;

namespace HueLighting.Models {
    class EntertainmentGroupManager : IEntertainmentGroupManager {
        private readonly IPreferences           mPreferences;
        private readonly Group                  mEntertainmentGroup;
        private StreamingHueClient              mStreamingClient;
        private StreamingGroup                  mStreamingGroup;
        private EntertainmentLayer              mBaseLayer;

        public EntertainmentGroupManager( IPreferences preferences, Group forGroup ) {
            mPreferences = preferences;
            mEntertainmentGroup = forGroup;
        }

        public async Task<bool> StartStreamingGroup() {
            bool    retValue;
            var     configuration = mPreferences.Load<HueConfiguration>();

            try {
                mStreamingClient = new StreamingHueClient( configuration.BridgeIp, configuration.BridgeAppKey, configuration.BridgeStreamingKey );
                mStreamingGroup = new StreamingGroup( mEntertainmentGroup.Locations );

                await mStreamingClient.Connect( mEntertainmentGroup.Id );
                mBaseLayer = mStreamingGroup.GetNewLayer( true );

                retValue = mBaseLayer != null;
            }
            catch( Exception ) {
                retValue = false;
            }

            return retValue;
        }
    }
}
