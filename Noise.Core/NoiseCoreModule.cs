using System.Collections.Generic;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Core.BackgroundTasks;
using Noise.Core.Configuration;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.DataExchange;
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

			NoiseLogger.Current.LogMessage( "==============================" );
			NoiseLogger.Current.LogMessage( "Noise Core Module loading." );
		}

		public void Initialize() {
			mContainer.RegisterInstance<IEventAggregator>( new EventAggregator(), new ContainerControlledLifetimeManager());

			mContainer.RegisterType<ICloudSyncManager, CloudSyncManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEqManager, EqManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILifecycleManager, LifecycleManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IStorageFileProcessor, StorageFileProcessor>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IMetaDataCleaner, MetaDataCleaner>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<INoiseManager, NoiseManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILibraryConfiguration, LibraryConfigurationManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayController, PlayController>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IScrobbler, PlayScrobbler>( new HierarchicalLifetimeManager());
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
			mContainer.RegisterType<IRequireConstruction, DataUpdates>( "DataUpdates", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRequireConstruction, FileUpdates>( "FileUpdates", new HierarchicalLifetimeManager());
//			mContainer.RegisterType<IRequireConstruction, LyricsSearcher>( "LyricsSearcher", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEnumerable<IRequireConstruction>, IRequireConstruction[]>();

			mContainer.RegisterType<IDomainSearchProvider, DomainSearchProvider>();
			mContainer.RegisterType<IStorageFolderSupport, StorageFolderSupport>();

			// Configure the default constructor for the RecurringTaskScheduler class.
			mContainer.RegisterType<RecurringTaskScheduler>( new InjectionConstructor());
			mContainer.RegisterType<IRecurringTaskScheduler, RecurringTaskScheduler>( new HierarchicalLifetimeManager());

			mContainer.RegisterType<IBackgroundTask, DiscographyExplorer>( "DiscographyExplorer" );
			mContainer.RegisterType<IBackgroundTask, DecadeTagBuilder>( "DecadeTagBuilder" );
			mContainer.RegisterType<IBackgroundTask, SearchBuilder>( "SearchBuilder" );
			mContainer.RegisterType<IBackgroundTask, MetadataUpdateTask>( "MetadataUpdate" );
			mContainer.RegisterType<IEnumerable<IBackgroundTask>, IBackgroundTask[]>();

			mContainer.RegisterType<ICloudSyncProvider, CloudSyncFavorites>( "SyncFavorites" );
			mContainer.RegisterType<ICloudSyncProvider, CloudSyncStreams>( "SyncStreams" );
			mContainer.RegisterType<IEnumerable<ICloudSyncProvider>, ICloudSyncProvider[]>();

			mContainer.RegisterType<IPlayStrategyFactory, PlayStrategyFactory>();

			mContainer.RegisterType<IPlayExhaustedFactory, PlayExhaustedFactory>();
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyCategory>( "CategoryExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyFavorites>( "FavoritesExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyArtist>( "ArtistExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyGenre>( "GenreExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyPlayList>( "PlayListExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyReplay>( "ReplayExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategySimilar>( "SimilarExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyStop>( "StopExhaustedStrategy" );
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyStream>( "StreamExhaustedStrategy" );
			mContainer.RegisterType<IEnumerable<IPlayExhaustedStrategy>, IPlayExhaustedStrategy[]>();

			mContainer.RegisterType<IPlayQueueSupport, PlayQueueRandomTracks>( "PlayQueueRandomTracks" );
			mContainer.RegisterType<IEnumerable<IPlayQueueSupport>, IPlayQueueSupport[]>();
		}
	}
}
