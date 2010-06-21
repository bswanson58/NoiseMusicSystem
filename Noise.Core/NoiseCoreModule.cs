using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.FileStore;
using Noise.Infrastructure;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseCoreModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IDatabaseManager, DatabaseManager>();
			mContainer.RegisterType<IDataProvider, DataProvider>();
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>();
			mContainer.RegisterType<IMetaDataExplorer, MetaDataExplorer>();
			mContainer.RegisterType<INoiseManager, NoiseManager>();
		}
	}
}
