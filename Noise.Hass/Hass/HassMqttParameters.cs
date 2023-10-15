// ReSharper disable IdentifierTypo

namespace Noise.Hass.Hass {
    public class HassMqttParameters {
        public bool MqttEnabled { get; set; }
        public string ServerAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseRetainFlag { get; set; }
        public string DeviceName { get; set; }
        public string ClientIdentifier { get; set; }

        public HassMqttParameters() {
            MqttEnabled = true;
            ServerAddress = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
            UseRetainFlag = false;
            DeviceName = string.Empty;
            ClientIdentifier = string.Empty;
        }
    }
}
