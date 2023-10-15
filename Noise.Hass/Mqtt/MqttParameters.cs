using System;

namespace Noise.Hass.Mqtt {
    public class MqttParameters {
        public  bool        MqttEnabled { get; set; }
        public  String      ServerAddress { get; set; }
        public  String      UserName {  get; set; }
        public  String      Password { get; set; }
        public  bool        UseRetainFlag { get; set; }

        public MqttParameters() {
            MqttEnabled = false;
            ServerAddress = String.Empty;
            UserName = String.Empty;
            Password = String.Empty;
            UseRetainFlag = true;
        }
    }
}
