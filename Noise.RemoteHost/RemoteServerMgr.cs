using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	public class RemoteServerMgr : IRemoteServer {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private INoiseRemoteData			mRemoteDataServer;
		private ServiceHost					mDataServerHost;
		private INoiseRemoteQueue			mRemoteQueueServer;
		private ServiceHost					mQueueServerHost;
		private INoiseRemoteSearch			mRemoteSearchServer;
		private ServiceHost					mSearchServerHost;

		public RemoteServerMgr( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public void OpenRemoteServer() {
			mRemoteDataServer = mContainer.Resolve<INoiseRemoteData>();
			mDataServerHost = new WebServiceHost( mRemoteDataServer );
			mDataServerHost.AddServiceEndpoint( typeof( INoiseRemoteData ), new WebHttpBinding(), new Uri( "http://localhost:88/Data" ));
			if(!OpenRemoteServer( mDataServerHost )) {
				mDataServerHost = null;
			}

			mRemoteQueueServer = mContainer.Resolve<INoiseRemoteQueue>();
			mQueueServerHost = new WebServiceHost( mRemoteQueueServer );
			mQueueServerHost.AddServiceEndpoint( typeof( INoiseRemoteQueue ) ,new WebHttpBinding(), new Uri( "http://localhost:88/Queue" ));
			if(!OpenRemoteServer( mQueueServerHost )) {
				mQueueServerHost = null;
			}

			mRemoteSearchServer = mContainer.Resolve<INoiseRemoteSearch>();
			mSearchServerHost = new WebServiceHost( mRemoteSearchServer );
			mSearchServerHost.AddServiceEndpoint( typeof( INoiseRemoteSearch ), new WebHttpBinding(), new Uri( "http://localhost:88/Search" ));
			if(!OpenRemoteServer( mSearchServerHost )) {
				mSearchServerHost = null;
			}
		}

		private bool OpenRemoteServer( ServiceHost host ) {
			bool	openSucceeded = false;

			try {
				host.Open();

				openSucceeded = true;
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteServerMgr:OpenRemoteServer", ex );
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

		private void CloseRemoteServer( ServiceHost host ) {
			if( host != null ) {
				bool	closeSucceeded = false;

				try {
					host.Close();

					closeSucceeded = true;
				}
				catch( Exception ex ) {
					mLog.LogException( "RemoteServerMgr:CloseRemoteServer", ex );
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
