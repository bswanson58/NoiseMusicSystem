using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.Configuration;
using Noise.Service.Infrastructure.Interfaces;
using NServiceBus;

namespace Noise.Service.Infrastructure.ServiceBus {
	public class ServiceBusManager : IServiceBusManager {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private	IBus						mMessageBus;

		public ServiceBusManager( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();

			MessageHandlerBase.EventAggregator = mContainer.Resolve<IEventAggregator>();
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
				mLog.LogException( "Exception - MessagePublisher InitializeSubscriber:", ex );
			}

			return( retValue );
		}

		public void Publish( IMessage message ) {
			mMessageBus.Publish( message );
		}
	}
}
