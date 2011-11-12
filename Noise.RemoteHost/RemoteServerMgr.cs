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

		public RemoteServerMgr( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public void OpenRemoteServer() {
			mRemoteDataServer = mContainer.Resolve<INoiseRemoteData>();
			mDataServerHost = new WebServiceHost( mRemoteDataServer );
			mDataServerHost.AddServiceEndpoint( typeof( INoiseRemoteData ), new WebHttpBinding(), new Uri( "http://localhost:88/Data" ));

			bool	openSucceeded = false;

			try {
				mDataServerHost.Open();

				openSucceeded = true;
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteServerMgr:OpenRemoteServer", ex );
			}
			finally {
				if(!openSucceeded ) {
					mDataServerHost.Abort();

					mDataServerHost = null;
				}
			}
		}

		public void CloseRemoteServer() {
			if( mDataServerHost != null ) {
				bool	closeSucceeded = false;

				try {
					mDataServerHost.Close();

					closeSucceeded = true;
				}
				catch( Exception ex ) {
					mLog.LogException( "RemoteServerMgr:CloseRemoteServer", ex );
				}
				finally {
					if(!closeSucceeded ) {
						mDataServerHost.Abort();
					}

					mDataServerHost = null;
				}
			}
		}
	}
}
