using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure;
using Noise.Infrastructure.RemoteHost;
using Noise.RemoteHost.Discovery;

namespace Noise.RemoteHost {
	public class RemoteServerMgr : IRemoteServer {
		private const string						cDiscoveryRealm = "NoiseMusicSystem";
		private const string						cDiscoveryAddress = "239.10.30.58";
		private const int							cDiscoveryPort = 6502;
		private const int							cDiscoveryResponsePort = cDiscoveryPort + 1;

		private readonly RemoteHostConfiguration	mHostConfiguration;
		private readonly INoiseRemote				mRemoteServer;
		private readonly string						mHostBaseAddress;
		private ServiceHost							mServerHost;
		private readonly INoiseRemoteData			mRemoteDataServer;
		private ServiceHost							mDataServerHost;
		private readonly INoiseRemoteQueue			mRemoteQueueServer;
		private ServiceHost							mQueueServerHost;
		private readonly INoiseRemoteSearch			mRemoteSearchServer;
		private ServiceHost							mSearchServerHost;
		private readonly INoiseRemoteTransport		mRemoteTransportServer;
		private ServiceHost							mTransportServerHost;
		private readonly INoiseRemoteLibrary		mRemoteLibraryServer;
		private ServiceHost							mLibrarySeverHost;
		private MulticastEndpoint					mDiscoveryListener;

		public RemoteServerMgr( RemoteHostConfiguration hostConfiguration, INoiseRemote noiseRemote, INoiseRemoteData noiseRemoteData,
								INoiseRemoteQueue noiseRemoteQueue, INoiseRemoteSearch noiseRemoteSearch,
								INoiseRemoteTransport noiseRemoteTransport, INoiseRemoteLibrary noiseRemoteLibrary ) {
			mHostConfiguration = hostConfiguration;
			mRemoteServer = noiseRemote;
			mRemoteDataServer = noiseRemoteData;
			mRemoteQueueServer = noiseRemoteQueue;
			mRemoteSearchServer = noiseRemoteSearch;
			mRemoteTransportServer = noiseRemoteTransport;
			mRemoteLibraryServer = noiseRemoteLibrary;

			mHostBaseAddress = string.Format( "http://localhost:{0}/Noise", mHostConfiguration.HostPort );
		}

		public void OpenRemoteServer() {
			mServerHost = new WebServiceHost( mRemoteServer );
			mServerHost.AddServiceEndpoint( typeof( INoiseRemote ), new WebHttpBinding(), new Uri( mHostBaseAddress ));
			if(!OpenRemoteServer( mServerHost )) {
				mServerHost = null;
			}

			mDataServerHost = new WebServiceHost( mRemoteDataServer );
			mDataServerHost.AddServiceEndpoint( typeof( INoiseRemoteData ), new WebHttpBinding(), new Uri( mHostBaseAddress + "/Data" ));
			if(!OpenRemoteServer( mDataServerHost )) {
				mDataServerHost = null;
			}

			mQueueServerHost = new WebServiceHost( mRemoteQueueServer );
			mQueueServerHost.AddServiceEndpoint( typeof( INoiseRemoteQueue ) ,new WebHttpBinding(), new Uri( mHostBaseAddress + "/Queue" ));
			if(!OpenRemoteServer( mQueueServerHost )) {
				mQueueServerHost = null;
			}

			mSearchServerHost = new WebServiceHost( mRemoteSearchServer );
			mSearchServerHost.AddServiceEndpoint( typeof( INoiseRemoteSearch ), new WebHttpBinding(), new Uri( mHostBaseAddress + "/Search" ));
			if(!OpenRemoteServer( mSearchServerHost )) {
				mSearchServerHost = null;
			}

			mTransportServerHost = new WebServiceHost( mRemoteTransportServer );
			mTransportServerHost.AddServiceEndpoint( typeof( INoiseRemoteTransport ), new WebHttpBinding(), new Uri( mHostBaseAddress + "/Transport" ));
			if(!OpenRemoteServer( mTransportServerHost )) {
				mTransportServerHost = null;
			}

			mLibrarySeverHost = new WebServiceHost( mRemoteLibraryServer );
			mLibrarySeverHost.AddServiceEndpoint( typeof( INoiseRemoteLibrary ), new WebHttpBinding(), new Uri( mHostBaseAddress + "/Library" ));
			if(!OpenRemoteServer( mLibrarySeverHost )) {
				mLibrarySeverHost = null;
			}

			StartDiscoveryListener();

			NoiseLogger.Current.LogMessage( "Remote services started." );
		}

		private void StartDiscoveryListener() {
			var	responseSender = new RequestSender( cDiscoveryResponsePort );
			var responder = new RequestResponder( cDiscoveryRealm, responseSender );
			var discoveryMessage = string.Format( "http://{0}:{1}", LocalIPAddress(), mHostConfiguration.HostPort );

			responder.AddRequestResponse( "ServerDiscovery", "ServerEndpoint", discoveryMessage );

			mDiscoveryListener = new MulticastEndpoint( IPAddress.Parse( cDiscoveryAddress ), cDiscoveryPort );
			mDiscoveryListener.StartListener( responder );

			NoiseLogger.Current.LogMessage( "ServerDiscovery started for endpoint: {0}", discoveryMessage );
		}

		private string LocalIPAddress() {
			var			retValue = string.Empty;
			IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName());

			foreach( var ip in host.AddressList ) {
				if( ip.AddressFamily == AddressFamily.InterNetwork ) {
					retValue = ip.ToString();

					break;
				}
			}

			return( retValue );
		}

		private static bool OpenRemoteServer( ServiceHost host ) {
			bool	openSucceeded = false;

			try {
				host.Open();

				openSucceeded = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteServerMgr:OpenRemoteServer", ex );
			}
			finally {
				if(!openSucceeded ) {
					host.Abort();
				}
			}

			return( openSucceeded );
		}

		public void CloseRemoteServer() {
			CloseRemoteServer( mDataServerHost );
			CloseRemoteServer( mQueueServerHost );
			CloseRemoteServer( mSearchServerHost );
			CloseRemoteServer( mTransportServerHost );
			CloseRemoteServer( mLibrarySeverHost );

			if( mDiscoveryListener != null ) {
				mDiscoveryListener.StopListener();
			}
		}

		private static void CloseRemoteServer( ServiceHost host ) {
			if( host != null ) {
				bool	closeSucceeded = false;

				try {
					host.Close();

					closeSucceeded = true;
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "RemoteServerMgr:CloseRemoteServer", ex );
				}
				finally {
					if(!closeSucceeded ) {
						host.Abort();
					}
				}
			}
		}
	}
}
