using System.Collections.Generic;
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
using Noise.Core.Support;
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
			mContainer.RegisterType<ICloudSyncManager, CloudSyncManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataProvider, DataProvider>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEqManager, EqManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILifecycleManager, LifecycleManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IMetaDataExplorer, MetaDataExplorer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IMetaDataCleaner, MetaDataCleaner>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<INoiseManager, NoiseManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayController, PlayController>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISearchProvider, LuceneSearchProvider>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ITagManager, TagManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<DatabaseStatistics, DatabaseStatistics>();
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();

			mContainer.RegisterType<IDbBaseProvider, DbBaseProvider>();
			mContainer.RegisterType<IArtistProvider, ArtistProvider>();
			mContainer.RegisterType<IAlbumProvider, AlbumProvider>();
			mContainer.RegisterType<ITrackProvider, TrackProvider>();
			mContainer.RegisterType<IInternetStreamProvider, InternetStreamProvider>();
			mContainer.RegisterType<IArtworkProvider, ArtworkProvider>();
			mContainer.RegisterType<IDiscographyProvider, DbDiscographyProvider>();
			mContainer.RegisterType<IDomainSearchProvider, DomainSearchProvider>();
			mContainer.RegisterType<IGenreProvider, GenreProvider>();
			mContainer.RegisterType<ILyricProvider, LyricProvider>();
			mContainer.RegisterType<IPlayHistoryProvider, PlayHistoryProvider>();
			mContainer.RegisterType<IPlayListProvider, PlayListProvider>();
			mContainer.RegisterType<IStorageFileProvider, StorageFileProvider>();
			mContainer.RegisterType<IStorageFolderProvider, StorageFolderProvider>();
			mContainer.RegisterType<ITagProvider, TagProvider>();
			mContainer.RegisterType<ITagAssociationProvider, TagAssociationProvider>();
			mContainer.RegisterType<ITextInfoProvider, TextInfoProvider>();
			mContainer.RegisterType<ITimestampProvider, TimestampProvider>();
			mContainer.RegisterType<IAssociatedItemListProvider, AssociatedItemListProvider>();

			mContainer.RegisterType<IRequireConstruction, BackgroundTaskManager>( "BackgroundTaskManager", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, ContentManager>( "ContentManager", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, DataUpdates>( "DataUpdates", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, FileUpdates>( "FileUpdates", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, LyricsSearcher>( "LyricsSearcher", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEnumerable<IRequireConstruction>, IRequireConstruction[]>();

			mContainer.RegisterType<IBackgroundTask, ContentBuilder>( "ContentBuilder" );
			mContainer.RegisterType<IBackgroundTask, DecadeTagBuilder>( "DecadeTagBuilder" );
			mContainer.RegisterType<IBackgroundTask, DiscographyExplorer>( "DiscographyExplorer" );
			mContainer.RegisterType<IBackgroundTask, LinkSimilarArtists>( "LinkSimilarArtists" );
			mContainer.RegisterType<IBackgroundTask, LinkTopAlbums>( "LinkTopAlbums" );
			mContainer.RegisterType<IBackgroundTask, SearchBuilder>( "SearchBuilder" );
			mContainer.RegisterType<IEnumerable<IBackgroundTask>, IBackgroundTask[]>();

			mContainer.RegisterType<ICloudSyncProvider, CloudSyncFavorites>( "SyncFavorites" );
			mContainer.RegisterType<ICloudSyncProvider, CloudSyncStreams>( "SyncStreams" );
			mContainer.RegisterType<IEnumerable<ICloudSyncProvider>, ICloudSyncProvider[]>();

			mContainer.RegisterType<IContentProvider, BandMembersProvider>( "BandMembersProvider" );
			mContainer.RegisterType<IContentProvider, BiographyProvider>( "BiographyProvider" );
			mContainer.RegisterType<IContentProvider, DiscographyProvider>( "DiscographyProvider" );
			mContainer.RegisterType<IContentProvider, SimilarArtistsProvider>( "SimilarArtists" );
			mContainer.RegisterType<IContentProvider, TopAlbumsProvider>( "TopAlbumsProvider" );
			mContainer.RegisterType<LastFmProvider, LastFmProvider>( new  HierarchicalLifetimeManager());
			mContainer.RegisterType<IEnumerable<IContentProvider>, IContentProvider[]>();

			mContainer.RegisterType<IPlayStrategyFactory, PlayStrategyFactory>();
			mContainer.RegisterType<PlayStrategySingle>();
			mContainer.RegisterType<PlayStrategyRandom>();
			mContainer.RegisterType<PlayStrategyTwoFers>();

			mContainer.RegisterType<IPlayExhaustedFactory, PlayExhaustedFactory>();
			mContainer.RegisterType<PlayExhaustedStrategyCategory>();
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
