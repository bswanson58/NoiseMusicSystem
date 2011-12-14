using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Core.BackgroundTasks;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.DataExchange;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Core.MediaPlayer;
using Noise.Core.PlayHistory;
using Noise.Core.PlayQueue;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseCoreModule( IUnityContainer container ) {
			mContainer = container;

			NoiseLogger.Current.LogMessage( "------------------------------" );
			NoiseLogger.Current.LogMessage( "Noise Core Module loading." );
		}

		public void Initialize() {
			mContainer.RegisterType<IAudioPlayer, AudioPlayer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IBackgroundTaskManager, BackgroundTaskManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ICloudSyncManager, CloudSyncManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IContentManager, ContentManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataProvider, DataProvider>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataUpdates, DataUpdates>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDatabaseManager, DatabaseManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEqManager, EqManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IFileUpdates, FileUpdates>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILyricsProvider, LyricsProvider>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IMetaDataExplorer, MetaDataExplorer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IMetaDataCleaner, MetaDataCleaner>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<INoiseManager, NoiseManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayListMgr, PlayListMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayController, PlayController>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISearchProvider, LuceneSearchProvider>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ITagManager, TagManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<DatabaseStatistics, DatabaseStatistics>();
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();

			mContainer.RegisterType<IPlayStrategyFactory, PlayStrategyFactory>();
			mContainer.RegisterType<PlayStrategySingle>();
			mContainer.RegisterType<PlayStrategyRandom>();
			mContainer.RegisterType<PlayStrategyTwoFers>();

			mContainer.RegisterType<IPlayExhaustedFactory, PlayExhaustedFactory>();
			mContainer.RegisterType<PlayExhaustedStrategyFavorites>();
			mContainer.RegisterType<PlayExhaustedStrategyGenre>();
			mContainer.RegisterType<PlayExhaustedStrategyPlayList>();
			mContainer.RegisterType<PlayExhaustedStrategyReplay>();
			mContainer.RegisterType<PlayExhaustedStrategySimilar>();
			mContainer.RegisterType<PlayExhaustedStrategyStop>();
			mContainer.RegisterType<PlayExhaustedStrategyStream>();
		}
	}
}
