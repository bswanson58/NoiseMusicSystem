using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteServer : INoiseRemote {
		private readonly IUnityContainer					mContainer;
		private	readonly IEventAggregator					mEvents;
		private	readonly Dictionary<string, ClientEvents>	mClientList;
		private readonly ILog								mLog;

		public RemoteServer( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mClientList = new Dictionary<string, ClientEvents>();
			mLog = mContainer.Resolve<ILog>();

			mEvents.GetEvent<Events.PlayQueueChanged>().Subscribe( OnQueueChanged );
		}

		public ServerVersion GetServerVersion() {
			return( new ServerVersion { Major = 1, Minor = 0, Build = 0, Revision = 1 });
		}

		public BaseResult RequestEvents( string address ) {
			var		retValue = new BaseResult();

			if( mClientList.ContainsKey( address )) {
				var client = mClientList[address];

				try {
					client.Close();
				}
				catch( Exception ex ) {
					mLog.LogException( "RemoteServer:client.Close:", ex );
				}

				mClientList.Remove( address );
			}

			if(!mClientList.ContainsKey( address )) {
				var client = new ClientEvents( new WebHttpBinding(), new EndpointAddress( address ));

				mClientList.Add( address, client );

				retValue.Success = true;
				mLog.LogInfo( "Added remote client: %s", address );
			}
			else {
				retValue.ErrorMessage = "Remote client address already registered.";
			}

			return( retValue );
		}

		public BaseResult RevokeEvents( string address ) {
			var		retValue = new BaseResult();

			if( mClientList.ContainsKey( address )) {
				mClientList.Remove( address );

				retValue.Success = true;
				mLog.LogInfo( "Removed remote client: %s", address );
			}
			else {
				retValue.ErrorMessage = "Client address not located in map.";
			}

			return( retValue );
		}

		private void OnQueueChanged( IPlayQueue queue ) {
			foreach( var client in mClientList.Values ) {
				client.EventInQueue();
			}
		}

		private void OnTransportChanged() {
			foreach( var client in mClientList.Values ) {
				client.EventInTransport();
			}
		}
	}
}
