using Noise.Hass.Discovery;
using Noise.Hass.Mqtt;
using Noise.Hass.Support;

// ReSharper disable IdentifierTypo

namespace Noise.Hass.Context {
    public interface IHassClientContext {
        DeviceConfigModel   DeviceConfiguration { get; }

        bool                MqttEnabled { get; }
        string              ServerAddress { get; }
        string              UserName { get; }
        string              Password { get; }
        bool                UseMqttRetainFlag { get; }

        string              LastWillTopic { get; }
        string              LastWillPayload { get; }

        string              DeviceAvailabilityTopic();
        string              DeviceMessageSubscriptionTopic();

        string              DeviceBaseTopic( string forDomain );
    }

    public class HassClientContext : IHassClientContext {
        private readonly MqttParameters     mMqttParameters;
        private readonly HassParameters     mHassParameters;

        public  DeviceConfigModel           DeviceConfiguration { get; }

        public  bool                        MqttEnabled => mMqttParameters.MqttEnabled;
        public  string                      ServerAddress => mMqttParameters.ServerAddress;
        public  string                      UserName => mMqttParameters.UserName;
        public  string                      Password => mMqttParameters.Password;
        public  bool                        UseMqttRetainFlag => mMqttParameters.UseRetainFlag;

        public HassClientContext( MqttParameters mqttParameters, HassParameters hassParameters,
                                  DeviceConfigModel deviceConfiguration ) {
            mMqttParameters = mqttParameters;
            mHassParameters = hassParameters;
            DeviceConfiguration = deviceConfiguration;
        }

        public string LastWillTopic =>
            $"{mHassParameters.DiscoveryPrefix}/{DeviceConfiguration.Name}/{DeviceConfiguration.Identifiers}/{Constants.Availability}";

        public string LastWillPayload =>
            Constants.Offline;

        public string DeviceBaseTopic( string forDomain ) =>
            $"{mHassParameters.DiscoveryPrefix}/{forDomain}/{DeviceConfiguration.Name}";

        public string DeviceAvailabilityTopic() =>
            $"{mHassParameters.DiscoveryPrefix}/{DeviceConfiguration.Name}/{DeviceConfiguration.Identifiers}/{Constants.Availability}";

        public string DeviceMessageSubscriptionTopic() =>
            $"{mHassParameters.DiscoveryPrefix}/+/{DeviceConfiguration.Name}/#";
    }
}
