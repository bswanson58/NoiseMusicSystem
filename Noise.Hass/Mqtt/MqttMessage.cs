using System;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

namespace Noise.Hass.Mqtt {
    public class MqttMessage {
        public  string      Topic { get; }
        public  string      Payload { get; }

        public MqttMessage() {
            Topic = String.Empty;
            Payload = String.Empty;
        }

        public MqttMessage( MqttApplicationMessage message ) {
            Topic = message.Topic;
            Payload = System.Text.Encoding.Default.GetString( message.Payload );
        }

        public MqttMessage( ManagedMqttApplicationMessage message ) {
            Topic = message.ApplicationMessage.Topic;
            Payload = String.Empty;
        }
    }
}
