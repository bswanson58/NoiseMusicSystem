using System.Collections.Generic;
using Caliburn.Micro;
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
using Prism.Ioc;
using Prism.Modularity;
using ReusableBits.Interfaces;
using ReusableBits.Threading;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly NoiseCorePreferences	mPreferences;

		public NoiseCoreModule( NoiseCorePreferences preferences ) {
			mPreferences = preferences;
		}

        public void RegisterTypes( IContainerRegistry containerRegistry ) {
			containerRegistry.RegisterInstance<IEventAggregator>( new EventAggregator());

			containerRegistry.Register<ILicenseManager, NoiseLicenseManager>();

			containerRegistry.Register<IArtistArtworkProvider, ArtistArtworkProvider>();
			containerRegistry.Register<IAlbumArtworkProvider, AlbumArtworkProvider>();
            containerRegistry.Register<ITagArtworkProvider, TagArtworkProvider>();

			containerRegistry.RegisterSingleton<IAudioController, AudioController>();
			containerRegistry.RegisterSingleton<IBackgroundTaskManager, BackgroundTaskManager>();
			containerRegistry.Register<IDataExchangeManager, DataExchangeManager>();
			containerRegistry.Register<IFolderExplorer, FolderExplorer>();
			containerRegistry.RegisterSingleton<ILifecycleManager, LifecycleManager>();
			containerRegistry.Register<IStorageFileProcessor, StorageFileProcessor>();
			containerRegistry.Register<IMetaDataCleaner, MetaDataCleaner>();
			containerRegistry.RegisterSingleton<INoiseManager, NoiseManager>();
			containerRegistry.RegisterSingleton<ILibraryConfiguration, LibraryConfigurationManager>();
			containerRegistry.RegisterSingleton<ILibraryBackupManager, LibraryBackupManager>();
			containerRegistry.Register<IPlayCommand, PlayCommand>();
			containerRegistry.RegisterSingleton<IPlayQueue, PlayQueueMgr>();
			containerRegistry.RegisterSingleton<IPlayHistory, PlayHistoryMgr>();
			containerRegistry.RegisterSingleton<IPlayController, PlayController>();
			containerRegistry.RegisterSingleton<IPlaybackContextManager, PlaybackContextManager>();
			containerRegistry.Register<IPlaybackContextWriter, PlaybackContextWriter>();
			containerRegistry.RegisterSingleton<ISearchProvider, LuceneSearchProvider>();
			containerRegistry.RegisterSingleton<ISidecarBuilder, SidecarBuilder>();
			containerRegistry.Register<ISidecarCreator, SidecarCreator>();
			containerRegistry.Register<ISidecarUpdater, SidecarWriter>();
			containerRegistry.Register<ISidecarWriter, SidecarWriter>();
            containerRegistry.Register<IStorageFolderSupport, StorageFolderSupport>();
			containerRegistry.RegisterSingleton<ITagManager, TagManager>();
            containerRegistry.Register<IUserTagManager, UserTagManager>();
			containerRegistry.Register<IRatings, RatingsUpdater>();
			containerRegistry.RegisterSingleton<ILibraryBuilder, LibraryBuilder>();
            containerRegistry.Register<ILibrarian, LibrarianModel>();
			containerRegistry.Register<DatabaseStatistics, DatabaseStatistics>();
			containerRegistry.Register<ISummaryBuilder, SummaryBuilder>();
            containerRegistry.Register<IDirectoryArchiver, DirectoryArchiver>();

			containerRegistry.Register<IPipelineStep, FileTypePipelineStep>( "FileTypePipelineStep" );
			containerRegistry.Register<IPipelineStep, CompletedPipelineStep>( "CompletedPipelineStep" );
			containerRegistry.Register<IPipelineStep, MusicProvidersPipelineStep>( "MusicProvidersPipelineStep" );
			containerRegistry.Register<IPipelineStep, ArtworkProvidersPipelineStep>( "ArtworkProvidersPipelineStep" );
			containerRegistry.Register<IPipelineStep, SidecarProvidersPipelineStep>( "SidecarProvidersPipelineStep" );
			containerRegistry.Register<IPipelineStep, InfoProvidersPipelineStep>( "InfoProvidersPipelineStep" );
			containerRegistry.Register<IPipelineStep, DetermineArtistPipelineStep>( "DetermineArtistPipelineStep" );
			containerRegistry.Register<IPipelineStep, DetermineAlbumPipelineStep>( "DetermineAlbumPipelineStep" );
			containerRegistry.Register<IPipelineStep, DetermineTrackPipelineStep>( "DetermineTrackNamePipelineStep" );
			containerRegistry.Register<IPipelineStep, DetermineVolumePipelineStep>( "DetermineVolumePipelineStep" );
			containerRegistry.Register<IPipelineStep, MusicMetadataPipelineStep>( "MusicMetadataPipelineStep" );
			containerRegistry.Register<IPipelineStep, UpdateMusicPipelineStep>( "UpdateMusicPipelineStep" );
			containerRegistry.Register<IPipelineStep, UpdateArtworkPipelineStep>( "UpdateArtworkPipelineStep" );
			containerRegistry.Register<IPipelineStep, UpdateInfoPipelineStep>( "UpdateInfoPipelineStep" );
			containerRegistry.Register<IPipelineStep, UpdateSidecarPipelineStep>( "UpdateSidecarPipelineStep" );
			containerRegistry.Register<IPipelineStep, UpdateUndeterminedPipelineStep>( "UpdateUndeterminedPipelineStep" );
            containerRegistry.Register<IList<IPipelineStep>, IPipelineStep[]>();

            containerRegistry.RegisterSingleton<IRequireConstruction, BackgroundTaskManager>( "BackgroundTaskManager" );
//			containerRegistry.RegisterSingleton<IRequireConstruction, LyricsSearcher>( "LyricsSearcher" );
			containerRegistry.Register<IList<IRequireConstruction>, IRequireConstruction[]>();

			containerRegistry.RegisterSingleton<IRecurringTaskScheduler, DefaultTaskScheduler>();

            containerRegistry.Register<IBackgroundTask, DiscographyExplorer>( "DiscographyExplorer" );
			containerRegistry.Register<IBackgroundTask, DecadeTagBuilder>( "DecadeTagBuilder" );
			containerRegistry.Register<IBackgroundTask, SearchBuilder>( "SearchBuilder" );
			containerRegistry.Register<IBackgroundTask, MetadataUpdateTask>( "MetadataUpdate" );
			containerRegistry.Register<IBackgroundTask, ReplayGainTask>( "ReplayGainTask" );
			if( mPreferences.MaintainArtistSidecars ) {
				containerRegistry.Register<IBackgroundTask, ArtistSidecarSync>( "ArtistSidecarSyncTask" );
			}
			if( mPreferences.MaintainAlbumSidecars ) {
				containerRegistry.Register<IBackgroundTask, AlbumSidecarSync>( "AlbumSidecarSyncTask" );
			}
			containerRegistry.Register<IList<IBackgroundTask>, IBackgroundTask[]>();

			containerRegistry.Register<IPlayStrategy, PlayStrategyFeaturedArtists>( ePlayStrategy.FeaturedArtists.ToString());
			containerRegistry.Register<IPlayStrategy, PlayStrategyNewReleases>( ePlayStrategy.NewReleases.ToString());
			containerRegistry.Register<IPlayStrategy, PlayStrategyRandom>( ePlayStrategy.Random.ToString());
			containerRegistry.Register<IPlayStrategy, PlayStrategySingle>( ePlayStrategy.Next.ToString());
			containerRegistry.Register<IPlayStrategy, PlayStrategyTwoFers>( ePlayStrategy.TwoFers.ToString());
			containerRegistry.Register<IList<IPlayStrategy>, IPlayStrategy[]>();
			containerRegistry.Register<IPlayStrategyFactory, PlayStrategyFactory>();

            containerRegistry.RegisterSingleton<IExhaustedStrategyPlayManager, ExhaustedStrategyPlayManager>();
            containerRegistry.Register<IExhaustedContextFactory, ExhaustedContextFactory>();
            containerRegistry.RegisterSingleton<IExhaustedStrategyFactory, ExhaustedStrategyFactory>();

            containerRegistry.Register<IExhaustedPlayHandler, StopPlay>( eTrackPlayHandlers.Stop.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, FavoriteTracks>( eTrackPlayHandlers.PlayFavorites.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, PlayArtistTracks>( eTrackPlayHandlers.PlayArtist.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, PlayGenre>( eTrackPlayHandlers.PlayGenre.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, ReplayQueue>( eTrackPlayHandlers.Replay.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, UserTaggedTracks>( eTrackPlayHandlers.PlayUserTags.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, RatedTracks>( eTrackPlayHandlers.RatedTracks.ToString());

            containerRegistry.Register<IExhaustedPlayHandler, AlreadyQueuedTracks>( eTrackPlayHandlers.AlreadyQueuedTracks.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, BadRatingTracks>( eTrackPlayHandlers.BadRatingTracks.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, DoNotPlayTracks>( eTrackPlayHandlers.DoNotPlayTracks.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, ShortTracks>( eTrackPlayHandlers.ShortTracks.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, TalkingTracks>( eTrackPlayHandlers.TalkingTracks.ToString());

            containerRegistry.Register<IExhaustedPlayHandler, HighlyRatedTracks>( eTrackPlayHandlers.HighlyRatedTracks.ToString());
            containerRegistry.Register<IExhaustedPlayHandler, PlayAdjacentTrack>( eTrackPlayHandlers.PlayAdjacentTracks.ToString());
            containerRegistry.Register<IList<IExhaustedPlayHandler>, IExhaustedPlayHandler[]>();

			containerRegistry.Register<IPlayQueueSupport, PlayQueueRandomTracks>( "PlayQueueRandomTracks" );
			containerRegistry.Register<IRandomTrackSelector, RandomTrackSelector>();
			containerRegistry.Register<IList<IPlayQueueSupport>, IPlayQueueSupport[]>();

			containerRegistry.Register<ILogBackgroundTasks, LogBackgroundTasks>();
			containerRegistry.Register<ILogBackup, LogBackup>();
			containerRegistry.Register<ILogLibraryBuilding, LogLibraryBuilding>();
			containerRegistry.Register<ILogLibraryBuildingDiscovery, LogLibraryBuildingDiscovery>();
			containerRegistry.Register<ILogLibraryCleaning, LogLibraryCleaning>();
			containerRegistry.Register<ILogLibraryClassification, LogLibraryClassification>();
			containerRegistry.Register<ILogLibraryBuildingSidecars, LogLibraryBuildingSidecars>();
			containerRegistry.Register<ILogLibraryBuildingSummary, LogLibraryBuildingSummary>();
			containerRegistry.Register<ILogPlayQueue, LogPlayQueue>();
			containerRegistry.Register<ILogPlayState, LogPlayState>();
			containerRegistry.Register<ILogPlayStrategy, LogPlayStrategy>();
			containerRegistry.Register<ILogLibraryConfiguration, LogLibraryConfiguration>();
			containerRegistry.Register<ILogUserStatus, LogUserStatus>();
		}

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
