using System.Collections.Generic;
using System.Configuration;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

namespace Noise.Service.Support {
	internal class CustomNServiceConfiguration : IConfigurationSource {
		public T GetConfiguration<T>() where T : class {
			if( typeof( T ) == typeof( UnicastBusConfig )) {
				return UnicastBusConfiguration() as T;
			}

			return ConfigurationManager.GetSection( typeof( T ).Name ) as T;
		}

		private static UnicastBusConfig UnicastBusConfiguration() {
			UnicastBusConfig	retValue = null;
			var	config = ConfigurationManager.GetSection( typeof( UnicastBusConfig ).Name ) as UnicastBusConfig;

			if( config != null ) {
				var	newMappings = new List<MessageEndpointMapping>();

				foreach( MessageEndpointMapping mapping in config.MessageEndpointMappings ) {
					if(!mapping.Endpoint.Contains( "@" )) {
						newMappings.Add( new MessageEndpointMapping { Endpoint = mapping.Endpoint + "@v-http", Messages = mapping.Messages });
					}
					else {
						newMappings.Add( mapping );
					}
				}
				var messageMappings = new MessageEndpointMappingCollection();
				foreach( var mapping in newMappings ) {
					messageMappings.Add( mapping );
				}

				retValue = new UnicastBusConfig { DistributorControlAddress = config.DistributorControlAddress,
					DistributorDataAddress =  config.DistributorDataAddress,
					ForwardReceivedMessagesTo = config.ForwardReceivedMessagesTo,
					MessageEndpointMappings = messageMappings };
			}

			return( retValue );
		}

	}
}
