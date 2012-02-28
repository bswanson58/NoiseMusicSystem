using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage {
	public class BlobStorageModule : IModule {
		private readonly IUnityContainer    mContainer;

		public BlobStorageModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IBlobStorageResolver, BlobStorageResolver>();
		}
	}
}
