using System;

namespace HueLighting.Dto {
    public class InstallationInfo {
        public  String  BridgeIp;
        public  String  BridgeId;
        public  String  BridgeUserName;

        public InstallationInfo() {
            BridgeIp = String.Empty;
            BridgeId = String.Empty;
            BridgeUserName = String.Empty;
        }
    }
}
