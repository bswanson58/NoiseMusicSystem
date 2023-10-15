
// ReSharper disable IdentifierTypo

using Noise.Hass.Support;

namespace Noise.Hass.Context {
    public class HassParameters {
        public string   DeviceName { get; set; }
        public string   ClientIdentifier { get; set; }
        public string   Model { get; set; }
        public string   Version { get; set; }
        public string   DiscoveryPrefix { get; set; }
        public string   DeviceSerialNumber { get; set; }

        public HassParameters() {
            DeviceName = string.Empty;
            ClientIdentifier = string.Empty;
            Model = string.Empty;
            Version = string.Empty;
            DiscoveryPrefix = Constants.DiscoveryPrefix;
            DeviceSerialNumber = string.Empty;
        }
    }
}
