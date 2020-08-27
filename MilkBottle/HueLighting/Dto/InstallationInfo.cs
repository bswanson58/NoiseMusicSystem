using System;

namespace HueLighting.Dto {
    public class InstallationInfo {
        public  String  BridgeIp;
        public  String  BridgeId;
        public  String  BridgeAppKey;

        public InstallationInfo() {
            BridgeIp = String.Empty;
            BridgeId = String.Empty;
            BridgeAppKey = String.Empty;
        }
    }
}
