using System.ServiceModel;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteServer : INoiseRemote {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;

		public RemoteServer( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public ServerVersion GetServerVersion() {
			return( new ServerVersion { Major = 1, Minor = 0, Build = 0, Revision = 1 });
		}
	}
}
