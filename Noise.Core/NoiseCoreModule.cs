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
using Noise.Infrastructure.Support;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseCoreModule( IUnityContainer container ) {
			mContainer = container;

			NoiseLogger.Current.LogMessage( "------------------------------" );
			NoiseLogger.Current.LogMessage( "Noise Core Module loading." );
		}

		public void Initialize() {
			mContainer.RegisterType<IAudioPlayer, AudioPlayer>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IBackgroundTaskManager, BackgroundTaskManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<ICloudSyncManager, CloudSyncManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IContentManager, ContentManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IDataProvider, DataProvider>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IDataUpdates, DataUpdates>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IDatabaseManager, DatabaseManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IEqManager, EqManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IFileUpdates, FileUpdates>( new PerResolveLifetimeManager());
//			mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>();
			mContainer.RegisterType<ILicenseManager, LicenseManager>();
			mContainer.RegisterType<ILyricsProvider, LyricsProvider>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IMetaDataExplorer, MetaDataExplorer>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IMetaDataCleaner, MetaDataCleaner>( new PerResolveLifetimeManager());
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();
			mContainer.RegisterType<INoiseManager, NoiseManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IPlayListMgr, PlayListMgr>( new PerResolveLifetimeManager());
			mContainer.RegisterType<IPlayController, PlayController>( new PerResolveLifetimeManager());
			mContainer.RegisterType<ISearchProvider, LuceneSearchProvider>( new PerResolveLifetimeManager());
			mContainer.RegisterType<ITagManager, TagManager>( new PerResolveLifetimeManager());
			mContainer.RegisterType<DatabaseStatistics, DatabaseStatistics>();

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
