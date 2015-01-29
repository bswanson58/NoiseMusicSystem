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
using Noise.Core.Logging;
using Noise.Core.PlayHistory;
using Noise.Core.PlayQueue;
using Noise.Core.PlayStrategies;
using Noise.Core.PlaySupport;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using ReusableBits.Threading;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseCoreModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterInstance<IEventAggregator>( new EventAggregator(), new ContainerControlledLifetimeManager());

			mContainer.RegisterType<ILicenseManager, NoiseLicenseManager>( new HierarchicalLifetimeManager());

			mContainer.RegisterType<IAudioController, AudioController>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>( new HierarchicalLifetimeManager());
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
			mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>( new HierarchicalLifetimeManager());
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
			mContainer.RegisterType<IBackgroundTask, ReplayGainTask>( "ReplayGainTask" );
			mContainer.RegisterType<IEnumerable<IBackgroundTask>, IBackgroundTask[]>();

			mContainer.RegisterType<IPlayStrategy, PlayStrategyFeaturedArtists>( ePlayStrategy.FeaturedArtists.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategyNewReleases>( ePlayStrategy.NewReleases.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategyRandom>( ePlayStrategy.Random.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategySingle>( ePlayStrategy.Next.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategyTwoFers>( ePlayStrategy.TwoFers.ToString());
			mContainer.RegisterType<IEnumerable<IPlayStrategy>, IPlayStrategy[]>();
			mContainer.RegisterType<IPlayStrategyFactory, PlayStrategyFactory>();

			mContainer.RegisterType<IPlayExhaustedFactory, PlayExhaustedFactory>();
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyCategory>( ePlayExhaustedStrategy.PlayCategory.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyFavorites>( ePlayExhaustedStrategy.PlayFavorites.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyArtist>( ePlayExhaustedStrategy.PlayArtist.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyArtistGenre>( ePlayExhaustedStrategy.PlayArtistGenre.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyPlayList>( ePlayExhaustedStrategy.PlayList.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyReplay>( ePlayExhaustedStrategy.Replay.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategySimilar>( ePlayExhaustedStrategy.PlaySimilar.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyStop>( ePlayExhaustedStrategy.Stop.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyStream>( ePlayExhaustedStrategy.PlayStream.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategySeldomPlayedArtists>( ePlayExhaustedStrategy.SeldomPlayedArtists.ToString());
			mContainer.RegisterType<IEnumerable<IPlayExhaustedStrategy>, IPlayExhaustedStrategy[]>();

			mContainer.RegisterType<IPlayQueueSupport, PlayQueueRandomTracks>( "PlayQueueRandomTracks" );
			mContainer.RegisterType<IRandomTrackSelector, RandomTrackSelector>();
			mContainer.RegisterType<IEnumerable<IPlayQueueSupport>, IPlayQueueSupport[]>();

			mContainer.RegisterType<ILogLibraryBuildingDiscovery, LogLibraryBuildingDiscovery>( new HierarchicalLifetimeManager());

			mContainer.RegisterInstance( NoiseLogger.Current );
		}
	}
}
