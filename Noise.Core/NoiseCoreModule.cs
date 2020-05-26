using System.Collections.Generic;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Core.BackgroundTasks;
using Noise.Core.Configuration;
using Noise.Core.Database;
using Noise.Core.Database.LuceneSearch;
using Noise.Core.DataBuilders;
using Noise.Core.DataExchange;
using Noise.Core.DataProviders;
using Noise.Core.FileProcessor;
using Noise.Core.FileStore;
using Noise.Core.Logging;
using Noise.Core.Platform;
using Noise.Core.PlayHistory;
using Noise.Core.PlayQueue;
using Noise.Core.PlayStrategies;
using Noise.Core.PlayStrategies.Exhausted;
using Noise.Core.PlayStrategies.Exhausted.BonusSuggesters;
using Noise.Core.PlayStrategies.Exhausted.Disqualifiers;
using Noise.Core.PlayStrategies.Exhausted.Suggesters;
using Noise.Core.PlaySupport;
using Noise.Core.Sidecars;
using Noise.Core.Support;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using ReusableBits.Threading;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer		mContainer;
		private readonly NoiseCorePreferences	mPreferences;

		public NoiseCoreModule( IUnityContainer container, NoiseCorePreferences preferences ) {
			mContainer = container;
			mPreferences = preferences;
		}

		public void Initialize() {
			mContainer.RegisterInstance<IEventAggregator>( new EventAggregator(), new ContainerControlledLifetimeManager());

			mContainer.RegisterType<ILicenseManager, NoiseLicenseManager>();

			mContainer.RegisterType<IArtistArtworkProvider, ArtistArtworkProvider>();
			mContainer.RegisterType<IAlbumArtworkProvider, AlbumArtworkProvider>();
            mContainer.RegisterType<ITagArtworkProvider, TagArtworkProvider>();

			mContainer.RegisterType<IAudioController, AudioController>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>();
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>();
			mContainer.RegisterType<ILifecycleManager, LifecycleManager>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IStorageFileProcessor, StorageFileProcessor>();
			mContainer.RegisterType<IMetaDataCleaner, MetaDataCleaner>();
			mContainer.RegisterType<INoiseManager, NoiseManager>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<ILibraryConfiguration, LibraryConfigurationManager>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<ILibraryBackupManager, LibraryBackupManager>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IPlayCommand, PlayCommand>();
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IPlayController, PlayController>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IPlaybackContextManager, PlaybackContextManager>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IPlaybackContextWriter, PlaybackContextWriter>();
			mContainer.RegisterType<IScrobbler, PlayScrobbler>();
			mContainer.RegisterType<ISearchProvider, LuceneSearchProvider>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<ISidecarBuilder, SidecarBuilder>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<ISidecarCreator, SidecarCreator>();
			mContainer.RegisterType<ISidecarUpdater, SidecarWriter>();
			mContainer.RegisterType<ISidecarWriter, SidecarWriter>();
            mContainer.RegisterType<IStorageFolderSupport, StorageFolderSupport>();
			mContainer.RegisterType<ITagManager, TagManager>( new ContainerControlledLifetimeManager());
            mContainer.RegisterType<IUserTagManager, UserTagManager>();
			mContainer.RegisterType<IRatings, RatingsUpdater>();
			mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>( new ContainerControlledLifetimeManager());
            mContainer.RegisterType<ILibrarian, LibrarianModel>();
			mContainer.RegisterType<DatabaseStatistics, DatabaseStatistics>();
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();
            mContainer.RegisterType<IDirectoryArchiver, DirectoryArchiver>();

			mContainer.RegisterType<IPipelineStep, FileTypePipelineStep>( "FileTypePipelineStep" );
			mContainer.RegisterType<IPipelineStep, CompletedPipelineStep>( "CompletedPipelineStep" );
			mContainer.RegisterType<IPipelineStep, MusicProvidersPipelineStep>( "MusicProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, ArtworkProvidersPipelineStep>( "ArtworkProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, SidecarProvidersPipelineStep>( "SidecarProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, InfoProvidersPipelineStep>( "InfoProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineArtistPipelineStep>( "DetermineArtistPipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineAlbumPipelineStep>( "DetermineAlbumPipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineTrackPipelineStep>( "DetermineTrackNamePipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineVolumePipelineStep>( "DetermineVolumePipelineStep" );
			mContainer.RegisterType<IPipelineStep, MusicMetadataPipelineStep>( "MusicMetadataPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateMusicPipelineStep>( "UpdateMusicPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateArtworkPipelineStep>( "UpdateArtworkPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateInfoPipelineStep>( "UpdateInfoPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateSidecarPipelineStep>( "UpdateSidecarPipelineStep" );
			mContainer.RegisterType<IPipelineStep, UpdateUndeterminedPipelineStep>( "UpdateUndeterminedPipelineStep" );
			mContainer.RegisterType<IEnumerable<IPipelineStep>, IPipelineStep[]>();

			mContainer.RegisterType<IRequireConstruction, BackgroundTaskManager>( "BackgroundTaskManager", new ContainerControlledLifetimeManager());
//			mContainer.RegisterType<IRequireConstruction, LyricsSearcher>( "LyricsSearcher", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEnumerable<IRequireConstruction>, IRequireConstruction[]>();

			// Configure the default constructor for the RecurringTaskScheduler class.
			mContainer.RegisterType<RecurringTaskScheduler>( new InjectionConstructor());
			mContainer.RegisterType<IRecurringTaskScheduler, RecurringTaskScheduler>( new HierarchicalLifetimeManager());

			mContainer.RegisterType<IBackgroundTask, DiscographyExplorer>( "DiscographyExplorer" );
			mContainer.RegisterType<IBackgroundTask, DecadeTagBuilder>( "DecadeTagBuilder" );
			mContainer.RegisterType<IBackgroundTask, SearchBuilder>( "SearchBuilder" );
			mContainer.RegisterType<IBackgroundTask, MetadataUpdateTask>( "MetadataUpdate" );
			mContainer.RegisterType<IBackgroundTask, ReplayGainTask>( "ReplayGainTask" );
			if( mPreferences.MaintainArtistSidecars ) {
				mContainer.RegisterType<IBackgroundTask, ArtistSidecarSync>( "ArtistSidecarSyncTask" );
			}
			if( mPreferences.MaintainAlbumSidecars ) {
				mContainer.RegisterType<IBackgroundTask, AlbumSidecarSync>( "AlbumSidecarSyncTask" );
			}
			mContainer.RegisterType<IEnumerable<IBackgroundTask>, IBackgroundTask[]>();

			mContainer.RegisterType<IPlayStrategy, PlayStrategyFeaturedArtists>( ePlayStrategy.FeaturedArtists.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategyNewReleases>( ePlayStrategy.NewReleases.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategyRandom>( ePlayStrategy.Random.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategySingle>( ePlayStrategy.Next.ToString());
			mContainer.RegisterType<IPlayStrategy, PlayStrategyTwoFers>( ePlayStrategy.TwoFers.ToString());
			mContainer.RegisterType<IEnumerable<IPlayStrategy>, IPlayStrategy[]>();
			mContainer.RegisterType<IPlayStrategyFactory, PlayStrategyFactory>();

            mContainer.RegisterType<IExhaustedStrategyPlayManager, ExhaustedStrategyPlayManager>( new ContainerControlledLifetimeManager());
            mContainer.RegisterType<IExhaustedContextFactory, ExhaustedContextFactory>();
            mContainer.RegisterType<IExhaustedStrategyFactory, ExhaustedStrategyFactory>( new ContainerControlledLifetimeManager());

            mContainer.RegisterType<IExhaustedPlayHandler, StopPlay>( eTrackPlayHandlers.Stop.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, FavoriteTracks>( eTrackPlayHandlers.PlayFavorites.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, PlayArtistTracks>( eTrackPlayHandlers.PlayArtist.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, PlayGenre>( eTrackPlayHandlers.PlayGenre.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, ReplayQueue>( eTrackPlayHandlers.Replay.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, UserTaggedTracks>( eTrackPlayHandlers.PlayUserTags.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, RatedTracks>( eTrackPlayHandlers.RatedTracks.ToString());

            mContainer.RegisterType<IExhaustedPlayHandler, AlreadyQueuedTracks>( eTrackPlayHandlers.AlreadyQueuedTracks.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, BadRatingTracks>( eTrackPlayHandlers.BadRatingTracks.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, DoNotPlayTracks>( eTrackPlayHandlers.DoNotPlayTracks.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, ShortTracks>( eTrackPlayHandlers.ShortTracks.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, TalkingTracks>( eTrackPlayHandlers.TalkingTracks.ToString());

            mContainer.RegisterType<IExhaustedPlayHandler, HighlyRatedTracks>( eTrackPlayHandlers.HighlyRatedTracks.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, PlayAdjacentTrack>( eTrackPlayHandlers.PlayAdjacentTracks.ToString());
            mContainer.RegisterType<IEnumerable<IExhaustedPlayHandler>, IExhaustedPlayHandler[]>();

			mContainer.RegisterType<IPlayQueueSupport, PlayQueueRandomTracks>( "PlayQueueRandomTracks" );
			mContainer.RegisterType<IRandomTrackSelector, RandomTrackSelector>();
			mContainer.RegisterType<IEnumerable<IPlayQueueSupport>, IPlayQueueSupport[]>();

			mContainer.RegisterType<ILogBackgroundTasks, LogBackgroundTasks>();
			mContainer.RegisterType<ILogBackup, LogBackup>();
			mContainer.RegisterType<ILogLibraryBuilding, LogLibraryBuilding>();
			mContainer.RegisterType<ILogLibraryBuildingDiscovery, LogLibraryBuildingDiscovery>();
			mContainer.RegisterType<ILogLibraryCleaning, LogLibraryCleaning>();
			mContainer.RegisterType<ILogLibraryClassification, LogLibraryClassification>();
			mContainer.RegisterType<ILogLibraryBuildingSidecars, LogLibraryBuildingSidecars>();
			mContainer.RegisterType<ILogLibraryBuildingSummary, LogLibraryBuildingSummary>();
			mContainer.RegisterType<ILogPlayQueue, LogPlayQueue>();
			mContainer.RegisterType<ILogPlayState, LogPlayState>();
			mContainer.RegisterType<ILogPlayStrategy, LogPlayStrategy>();
			mContainer.RegisterType<ILogLibraryConfiguration, LogLibraryConfiguration>();
			mContainer.RegisterType<ILogUserStatus, LogUserStatus>();
		}
	}
}
