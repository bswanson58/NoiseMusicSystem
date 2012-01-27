using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteServer : INoiseRemote, IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackTrackStarted> {
		private readonly ICaliburnEventAggregator			mEventAggregator;
		private	readonly Dictionary<string, ClientEvents>	mClientList;

		public RemoteServer( ICaliburnEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
			mClientList = new Dictionary<string, ClientEvents>();

			mEventAggregator.Subscribe( this );
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
					NoiseLogger.Current.LogException( "RemoteServer:client.Close:", ex );
				}

				mClientList.Remove( address );
			}

			if(!mClientList.ContainsKey( address )) {
				var client = new ClientEvents( new WebHttpBinding(), new EndpointAddress( address ));

				mClientList.Add( address, client );

				retValue.Success = true;
				NoiseLogger.Current.LogInfo( "Added remote client: %s", address );
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
				NoiseLogger.Current.LogInfo( "Removed remote client: %s", address );
			}
			else {
				retValue.ErrorMessage = "Client address not located in map.";
			}

			return( retValue );
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			// decouple from the event thread.
			new Task( OnQueueChangedTask ).Start();
		}

		public void Handle( Events.PlaybackTrackStarted eventArgs ) {
			// decouple from the event thread.
			new Task( OnQueueChangedTask ).Start();
		}

		private void OnQueueChangedTask() {
			foreach( var client in mClientList.Values ) {
				client.EventInQueue();
			}
		}

/*		private void OnTransportChanged() {
			foreach( var client in mClientList.Values ) {
				client.EventInTransport();
			}
		} */
	}
}
