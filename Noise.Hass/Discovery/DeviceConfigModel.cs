using System;
using System.Collections.Generic;

namespace Noise.Hass.Discovery {
    /// <summary>
    /// This information will be used when announcing this device on the mqtt topic
    /// </summary>
    public class DeviceConfigModel {
        /// <summary>
        /// (Optional) A list of connections of the device to the outside world as a list of
        /// tuples [connection_type, connection_identifier]. For example the MAC address of a
        /// network interface: "connections": [["mac", "02:5b:26:a8:dc:12"]].
        /// </summary>
        /// <value></value>
//        [JsonPropertyName( "connections" )]
        public ICollection<Tuple<string, string>> Connections { get; set; }

        /// <summary>
        /// (Optional) An Id to identify the device. For example a serial number.
        /// </summary>
        /// <value></value>
//        [JsonPropertyName( "identifiers" )]
        public string Identifiers { get; set; }

        /// <summary>
        /// (Optional) The manufacturer of the device.
        /// </summary>
        /// <value></value>
//        [JsonPropertyName( "manufacturer" )]
        public string Manufacturer { get; set; }

        /// <summary>
        /// (Optional) The model of the device.
        /// </summary>
        /// <value></value>
//        [JsonPropertyName( "model" )]
        public string Model { get; set; }

        /// <summary>
        /// (Optional) The name of the device.
        /// </summary>
        /// <value></value>
//        [JsonPropertyName( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// (Optional) The firmware version of the device.
        /// </summary>
        /// <value></value>
//        [JsonPropertyName( "sw_version" )]
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// (Optional) Identifier of a device that routes messages between this device and Home Assistant.
        /// Examples of such devices are hubs, or parent devices of a sub-device.
        /// This is used to show device topology in Home Assistant.
        /// </summary>
        /// <value></value>
//        [JsonPropertyName( "via_device" )]
        public string ViaDevice { get; set; }
    }
}
