using System;

namespace MilkBottle.Infrastructure.Dto {
    public class HueConfiguration {
        public  String  BridgeIp;
        public  String  BridgeId;
        public  String  BridgeAppKey;
        public  String  BridgeStreamingKey;
        public  String  EntertainmentGroupId;

        public HueConfiguration() {
            BridgeIp = String.Empty;
            BridgeId = String.Empty;
            BridgeAppKey = String.Empty;
            BridgeStreamingKey = String.Empty;
            EntertainmentGroupId = String.Empty;
        }
    }
}
