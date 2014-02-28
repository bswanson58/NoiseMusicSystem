using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	public class RemoteHostModule : IModule {
		private readonly IUnityContainer    mContainer;

		public RemoteHostModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IRemoteServer, RemoteServerMgr>();
			mContainer.RegisterType<INoiseRemote, RemoteServer>();
			mContainer.RegisterType<INoiseRemoteData, RemoteDataServer>();
			mContainer.RegisterType<INoiseRemoteQueue, RemoteQueueServer>();
			mContainer.RegisterType<INoiseRemoteSearch, RemoteSearchServer>();
			mContainer.RegisterType<INoiseRemoteTransport, RemoteTransportServer>();

			MappingConfiguration.Configure();
		}
	}
}
