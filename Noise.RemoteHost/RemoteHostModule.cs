using Noise.Infrastructure.RemoteHost;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.RemoteHost {
	public class RemoteHostModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
			containerRegistry.RegisterSingleton<IRemoteServer, RemoteServerMgr>();
			containerRegistry.Register<INoiseRemote, RemoteServer>();
			containerRegistry.Register<INoiseRemoteData, RemoteDataServer>();
			containerRegistry.Register<INoiseRemoteQueue, RemoteQueueServer>();
			containerRegistry.Register<INoiseRemoteSearch, RemoteSearchServer>();
			containerRegistry.Register<INoiseRemoteTransport, RemoteTransportServer>();
			containerRegistry.Register<INoiseRemoteLibrary, RemoteLibraryManager>();
		}

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
