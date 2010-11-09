using System.Collections.Generic;
using System.Configuration;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

namespace Noise.Service.Infrastructure.Configuration {
	public class CustomNServiceConfiguration : IConfigurationSource {
		private	readonly string		mServerName;

		public CustomNServiceConfiguration( string servernName ) {
			mServerName = servernName;
		}

		public T GetConfiguration<T>() where T : class {
			if( typeof( T ) == typeof( UnicastBusConfig )) {
				return UnicastBusConfiguration() as T;
			}

			return ConfigurationManager.GetSection( typeof( T ).Name ) as T;
		}

		private UnicastBusConfig UnicastBusConfiguration() {
			UnicastBusConfig	retValue = null;
			var	config = ConfigurationManager.GetSection( typeof( UnicastBusConfig ).Name ) as UnicastBusConfig;

			if( config != null ) {
				var	newMappings = new List<MessageEndpointMapping>();

				foreach( MessageEndpointMapping mapping in config.MessageEndpointMappings ) {
					if((!mapping.Endpoint.Contains( "@" )) &&
					   (!string.IsNullOrWhiteSpace( mServerName ))) {
						newMappings.Add( new MessageEndpointMapping { Endpoint = mapping.Endpoint + "@" + mServerName, Messages = mapping.Messages });
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
