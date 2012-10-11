using System.Collections.Generic;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Core.BackgroundTasks;
using Noise.Core.Configuration;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.DataExchange;
using Noise.Core.DataProviders;
using Noise.Core.FileProcessor;
using Noise.Core.FileStore;
using Noise.Core.MediaPlayer;
using Noise.Core.PlayHistory;
using Noise.Core.PlayQueue;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Threading;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseCoreModule( IUnityContainer container ) {
			mContainer = container;

			NoiseLogger.Current.LogMessage( "------------------------------" );
			NoiseLogger.Current.LogMessage( "Noise Core Module loading." );
		}

		public void Initialize() {
			mContainer.RegisterInstance<IEventAggregator>( new EventAggregator(), new ContainerControlledLifetimeManager());

			mContainer.RegisterType<IAudioPlayer, AudioPlayer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ICloudSyncManager, CloudSyncManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEqManager, EqManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILifecycleManager, LifecycleManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IStorageFileProcessor, StorageFileProcessor>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IMetaDataCleaner, MetaDataCleaner>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<INoiseManager, NoiseManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayController, PlayController>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISearchProvider, LuceneSearchProvider>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ITagManager, TagManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<DatabaseStatistics, DatabaseStatistics>();
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();

			mContainer.RegisterType<IPipelineStep, FileTypePipelineStep>( "FileTypePipelineStep" );
			mContainer.RegisterType<IPipelineStep, CompletedPipelineStep>( "CompletedPipelineStep" );
			mContainer.RegisterType<IPipelineStep, MusicProvidersPipelineStep>( "MusicProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, ArtworkProvidersPipelineStep>( "ArtworkProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, InfoProvidersPipelineStep>( "InfoProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineArtistPipelineStep>( "DetermineAtistPipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineAlbumPipelineStep>( "DetermineAlbumPipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineTrackPipelineStep>( "DetermineTrackNamePipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineVolumePipelineStep>( "DetermineVolumePipelineStep" );
			mContainer.RegisterType<IPipelineStep, MusicMetadataPipelineStep>( "MusicMetadataPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateMusicPipelineStep>( "UpdateMusicPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateArtworkPipelineStep>( "UpdateArtworkPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateInfoPipelineStep>( "UpdateInfoPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateUndeterminedPipelineStep>( "UpdateUndeterminedPipelineStep" );
			mContainer.RegisterType<IEnumerable<IPipelineStep>, IPipelineStep[]>();

			mContainer.RegisterType<IRequireConstruction, BackgroundTaskManager>( "BackgroundTaskManager", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, ContentManager>( "ContentManager", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, DataUpdates>( "DataUpdates", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, FileUpdates>( "FileUpdates", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, LyricsSearcher>( "LyricsSearcher", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, LibraryConfigurationManager>( "Configuration", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEnumerable<IRequireConstruction>, IRequireConstruction[]>();

			mContainer.RegisterType<IDomainSearchProvider, DomainSearchProvider>();
			mContainer.RegisterType<IStorageFolderSupport, StorageFolderSupport>();

			// Configure the default constructor for the RecurringTaskScheduler class.
			mContainer.RegisterType<RecurringTaskScheduler>( new InjectionConstructor());
			mContainer.RegisterType<IRecurringTaskScheduler, RecurringTaskScheduler>( new HierarchicalLifetimeManager());

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
