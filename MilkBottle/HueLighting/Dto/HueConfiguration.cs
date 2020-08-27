using System;

namespace HueLighting.Dto {
    public class HueConfiguration {
        public  String  BridgeIp;
        public  String  BridgeId;
        public  String  BridgeAppKey;

        public HueConfiguration() {
            BridgeIp = String.Empty;
            BridgeId = String.Empty;
            BridgeAppKey = String.Empty;
        }
    }
}
