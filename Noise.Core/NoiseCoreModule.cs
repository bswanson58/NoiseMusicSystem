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
using Noise.Core.Logging;
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

			mContainer.RegisterType<ILicenseManager, NoiseLicenseManager>( new HierarchicalLifetimeManager());

			mContainer.RegisterType<IArtistArtworkProvider, ArtistArtworkProvider>();
			mContainer.RegisterType<IAlbumArtworkProvider, AlbumArtworkProvider>();

			mContainer.RegisterType<IAudioController, AudioController>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDataExchangeManager, DataExchangeManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILifecycleManager, LifecycleManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IStorageFileProcessor, StorageFileProcessor>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IMetaDataCleaner, MetaDataCleaner>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<INoiseManager, NoiseManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILibraryConfiguration, LibraryConfigurationManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayCommand, PlayCommand>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlayController, PlayController>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlaybackContextManager, PlaybackContextManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IPlaybackContextWriter, PlaybackContextWriter>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IScrobbler, PlayScrobbler>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISearchProvider, LuceneSearchProvider>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISidecarBuilder, SidecarBuilder>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISidecarCreator, SidecarCreator>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISidecarUpdater, SidecarWriter>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISidecarWriter, SidecarWriter>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ITagManager, TagManager>( new HierarchicalLifetimeManager());
            mContainer.RegisterType<IUserTagManager, UserTagManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IRatings, RatingsUpdater>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<DatabaseStatistics, DatabaseStatistics>();
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();

			mContainer.RegisterType<IPipelineStep, FileTypePipelineStep>( "FileTypePipelineStep" );
			mContainer.RegisterType<IPipelineStep, CompletedPipelineStep>( "CompletedPipelineStep" );
			mContainer.RegisterType<IPipelineStep, MusicProvidersPipelineStep>( "MusicProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, ArtworkProvidersPipelineStep>( "ArtworkProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, SidecarProvidersPipelineStep>( "SidecarProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, InfoProvidersPipelineStep>( "InfoProvidersPipelineStep" );
			mContainer.RegisterType<IPipelineStep, DetermineArtistPipelineStep>( "DetermineAtistPipelineStep" );
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

			mContainer.RegisterType<IRequireConstruction, BackgroundTaskManager>( "BackgroundTaskManager", new HierarchicalLifetimeManager());
//			mContainer.RegisterType<IRequireConstruction, LyricsSearcher>( "LyricsSearcher", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IEnumerable<IRequireConstruction>, IRequireConstruction[]>();

			mContainer.RegisterType<IStorageFolderSupport, StorageFolderSupport>();

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

			mContainer.RegisterType<IPlayExhaustedFactory, PlayExhaustedFactory>();
/*			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyCategory>( ePlayExhaustedStrategy.PlayCategory.ToString());
		    mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyFavorites>( ePlayExhaustedStrategy.PlayFavorites.ToString());
		    mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyFavoritesPlus>( ePlayExhaustedStrategy.PlayFavoritesPlus.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyArtist>( ePlayExhaustedStrategy.PlayArtist.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyArtistGenre>( ePlayExhaustedStrategy.PlayArtistGenre.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyPlayList>( ePlayExhaustedStrategy.PlayList.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyReplay>( ePlayExhaustedStrategy.Replay.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategySimilar>( ePlayExhaustedStrategy.PlaySimilar.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyStop>( ePlayExhaustedStrategy.Stop.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyStream>( ePlayExhaustedStrategy.PlayStream.ToString());
			mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategySeldomPlayedArtists>( ePlayExhaustedStrategy.SeldomPlayedArtists.ToString());
            mContainer.RegisterType<IPlayExhaustedStrategy, PlayExhaustedStrategyUserTags>( ePlayExhaustedStrategy.PlayUserTags.ToString());
*/
            mContainer.RegisterType<IEnumerable<IPlayExhaustedStrategy>, IPlayExhaustedStrategy[]>();

            mContainer.RegisterType<IExhaustedStrategyPlayManager, ExhaustedStrategyPlayManager>( new HierarchicalLifetimeManager());
            mContainer.RegisterType<IExhaustedContextFactory, ExhaustedContextFactory>( new HierarchicalLifetimeManager());
            mContainer.RegisterType<IExhaustedStrategyFactory, ExhaustedStrategyFactory>( new HierarchicalLifetimeManager());

            mContainer.RegisterType<IExhaustedPlayHandler, StopPlay>( eTrackPlayHandlers.Stop.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, FavoriteTracks>( eTrackPlayHandlers.PlayFavorites.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, PlayArtistTracks>( eTrackPlayHandlers.PlayArtist.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, ReplayQueue>( eTrackPlayHandlers.Replay.ToString());
            mContainer.RegisterType<IExhaustedPlayHandler, UserTaggedTracks>( eTrackPlayHandlers.PlayUserTags.ToString());

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

			mContainer.RegisterType<ILogBackgroundTasks, LogBackgroundTasks>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogLibraryBuilding, LogLibraryBuilding>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogLibraryBuildingDiscovery, LogLibraryBuildingDiscovery>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogLibraryCleaning, LogLibraryCleaning>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogLibraryClassification, LogLibraryClassification>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogLibraryBuildingSidecars, LogLibraryBuildingSidecars>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogLibraryBuildingSummary, LogLibraryBuildingSummary>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogPlayQueue, LogPlayQueue>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogPlayState, LogPlayState>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogPlayStrategy, LogPlayStrategy>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ILogLibraryConfiguration, LogLibraryConfiguration>( new HierarchicalLifetimeManager());

			mContainer.RegisterType<ILogUserStatus, LogUserStatus>( new HierarchicalLifetimeManager());
		}
	}
}
