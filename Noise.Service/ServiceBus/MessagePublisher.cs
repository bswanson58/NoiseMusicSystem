using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Support;
using NServiceBus;
using NServiceBus.Saga;

namespace Noise.Service.ServiceBus {
	public class MessagePublisher {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly ILog				mLog;
		private	IBus						mMessageBus;

		public MessagePublisher( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();
		}

		public bool InitializePublisher() {
			var retValue = false;

			try {
				mMessageBus = Configure.With()
					.Log4Net()
					.UnityBuilder( mContainer )
					.XmlSerializer()
					.MsmqTransport()
						.IsTransactional( true )
					.MsmqSubscriptionStorage()
					.UnicastBus()
					.LoadMessageHandlers()
					.CreateBus()
					.Start();

				retValue = true;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - MessagePublisher InitializePublisher:", ex );
			}

			return( retValue );
		}

		public bool InitializeSubscriber() {
			var retValue = false;

			try {
				var	assembly = new List<Assembly>{ Assembly.GetAssembly( typeof( MessagePublisher )),
												   Assembly.GetAssembly( typeof( NServiceBus.Unicast.Transport.CompletionMessage )) };
				
				mMessageBus = Configure.With( assembly )
					.CustomConfigurationSource( new CustomNServiceConfiguration())
					.UnityBuilder()
					.MsmqSubscriptionStorage()
					.XmlSerializer()
					.MsmqTransport()
						.IsTransactional( true )
						.PurgeOnStartup( true )
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

		/// <summary>
		/// Returns true if the given type is a message handler.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool IsMessageHandler( Type t ) {
			if( t.IsAbstract )
				return false;

			if( typeof( ISaga ).IsAssignableFrom( t ) )
				return false;

			foreach( Type interfaceType in t.GetInterfaces() ) {
				Type messageType = GetMessageTypeFromMessageHandler( interfaceType );
				if( messageType != null )
					return true;
			}

			return false;
		}
		/// <summary>
		/// Returns the message type handled by the given message handler type.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Type GetMessageTypeFromMessageHandler( Type t ) {
			if( t.IsGenericType ) {
				Type[] args = t.GetGenericArguments();
				if( args.Length != 1 )
					return null;

				if( !typeof( IMessage ).IsAssignableFrom( args[0] ) )
					return null;

				Type handlerType = typeof( IMessageHandler<> ).MakeGenericType( args[0] );
				if( handlerType.IsAssignableFrom( t ) )
					return args[0];
			}

			return null;
		}

		public void StartPublisher() {
			if( mMessageBus != null ) {
				mEvents.GetEvent<Events.LibraryUpdateStarted>().Subscribe( OnLibraryUpdateStarted );
				mEvents.GetEvent<Events.LibraryUpdateCompleted>().Subscribe( OnLibraryUpdateCompleted );
			}
		}

		private void OnLibraryUpdateStarted( object sender ) {
			mMessageBus.Publish( new ServiceStatusMessage( "Library Update Started." ));
		}

		private void OnLibraryUpdateCompleted( object sender ) {
			mMessageBus.Publish( new ServiceStatusMessage( "Library Update Completed." ));
		}
	}
}
