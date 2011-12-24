using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.EloqueraDatabase.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraDatabaseModule : IModule {
		private readonly IUnityContainer    mContainer;

		public EloqueraDatabaseModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IDatabaseFactory, EloqueraDatabaseFactory>();
			mContainer.RegisterType<IBlobStorageResolver, BlobStorageResolver>();
			mContainer.RegisterType<IDatabaseManager, DatabaseManager>( new HierarchicalLifetimeManager());
		}
	}
}
