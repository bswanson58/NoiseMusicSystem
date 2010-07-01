using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.FileStore;
using Noise.Core.MediaPlayer;
using Noise.Core.PlayHistory;
using Noise.Core.PlayQueue;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseCoreModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IAudioPlayer, AudioPlayer>();
			mContainer.RegisterType<IDatabaseManager, DatabaseManager>();
			mContainer.RegisterType<IDataProvider, DataProvider>();
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>();
			mContainer.RegisterType<IMetaDataExplorer, MetaDataExplorer>();
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();
			mContainer.RegisterType<INoiseManager, NoiseManager>();
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>();
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>();
		}
	}
}
