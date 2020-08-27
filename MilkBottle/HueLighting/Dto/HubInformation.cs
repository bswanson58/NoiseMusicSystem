using System;
using System.Linq;
using Q42.HueApi;
using Q42.HueApi.Models.Bridge;

namespace HueLighting.Dto {
    public class HubInformation {
        public  string  BridgeId {  get; }
        public  string  IpAddress { get; }
        public  string  BridgeName { get; }
        public  string  BridgeAppKey {  get; }
        public  bool    IsConfiguredHub { get; }
        public  bool    IsAppRegistered { get; }
        public  bool    IsStreamingActive { get; }
        public  int     BulbCount {  get; }
        public  int     GroupCount { get; }

        public HubInformation( LocatedBridge bridge ) {
            IpAddress = bridge.IpAddress;
            BridgeId = bridge.BridgeId;

            BridgeName = String.Empty;

            IsAppRegistered = false;
        }

        public HubInformation( LocatedBridge bridge, Bridge bridgeInfo, string bridgeAppKey, bool isConfiguredHub ) {
            IpAddress = bridge.IpAddress;
            BridgeId = bridge.BridgeId;
            BridgeAppKey = bridgeAppKey;

            BridgeName = bridgeInfo.Config.Name;
            IsStreamingActive = bridgeInfo.IsStreamingActive;
            BulbCount = bridgeInfo.Lights.Count();
            GroupCount = bridgeInfo.Groups.Count();

            IsConfiguredHub = isConfiguredHub;
            IsAppRegistered = true;
        }

        public HubInformation( HubInformation fromHub, string appKey ) {
            BridgeId = fromHub.BridgeId;
            IpAddress = fromHub.IpAddress;
            BridgeName = fromHub.BridgeName;

            BridgeAppKey = appKey;

            IsConfiguredHub = fromHub.IsConfiguredHub;
            IsAppRegistered = fromHub.IsAppRegistered;
            IsStreamingActive = fromHub.IsStreamingActive;
            BulbCount = fromHub.BulbCount;
            GroupCount = fromHub.GroupCount;
        }
    }
}
