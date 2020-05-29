using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure.Interfaces;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.BlobStorage {
	public class BlobStorageModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IBlobStorageProvider, BlobStorageProvider>();
            containerRegistry.Register<IBlobStorageResolver, BlobStorageResolver>();
            containerRegistry.Register<IBlobStorageManager, BlobStorageManager>();
            containerRegistry.Register<IInPlaceStorage, InPlaceStorageProvider>();
            containerRegistry.RegisterSingleton<IInPlaceStorageManager, InPlaceStorageManager>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
