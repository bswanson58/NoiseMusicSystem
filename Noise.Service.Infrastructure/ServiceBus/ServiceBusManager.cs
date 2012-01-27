using System;
using System.Collections.Generic;
using System.Reflection;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Service.Infrastructure.Configuration;
using Noise.Service.Infrastructure.Interfaces;
using NServiceBus;

namespace Noise.Service.Infrastructure.ServiceBus {
	public class ServiceBusManager : IServiceBusManager {
		private	IBus	mMessageBus;

		public ServiceBusManager( ICaliburnEventAggregator eventAggregator ) {
			MessageHandlerBase.EventAggregator = eventAggregator;
		}

		public bool InitializeServer() {
			return( Initialize( string.Empty ));
		}

		public bool Initialize( string serverName ) {
			var retValue = false;

			try {
				var	assembly = new List<Assembly>{ Assembly.GetAssembly( GetType()),
												   Assembly.GetAssembly( typeof( NServiceBus.Unicast.Transport.CompletionMessage )) };
				
				mMessageBus = Configure.With( assembly )
					.Log4Net()
					.CustomConfigurationSource( new CustomNServiceConfiguration( serverName ))
					.DefaultBuilder()
					.MsmqSubscriptionStorage()
					.XmlSerializer()
					.MsmqTransport()
						.IsTransactional( true )
						.PurgeOnStartup( false )
					.UnicastBus()
					.LoadMessageHandlers()
					.CreateBus()
					.Start();
				
				retValue = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - MessagePublisher InitializeSubscriber:", ex );
			}

			return( retValue );
		}

		public void Publish( IMessage message ) {
			mMessageBus.Publish( message );
		}
	}
}
