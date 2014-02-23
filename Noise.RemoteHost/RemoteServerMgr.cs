using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	public class RemoteServerMgr : IRemoteServer {
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
		private ServiceDiscovery					mServiceDiscovery;

		public RemoteServerMgr( RemoteHostConfiguration hostConfiguration, INoiseRemote noiseRemote, INoiseRemoteData noiseRemoteData,
								INoiseRemoteQueue noiseRemoteQueue, INoiseRemoteSearch noiseRemoteSearch ) {
			mHostConfiguration = hostConfiguration;
			mRemoteServer = noiseRemote;
			mRemoteDataServer = noiseRemoteData;
			mRemoteQueueServer = noiseRemoteQueue;
			mRemoteSearchServer = noiseRemoteSearch;

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

			mServiceDiscovery = new ServiceDiscovery();
			if( mServiceDiscovery.Initialize()) {
				mServiceDiscovery.RegisterService( "_Noise._Tcp.", "Noise.Desktop", mHostConfiguration.HostPort );
			}

			NoiseLogger.Current.LogMessage( "Remote services started." );
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
