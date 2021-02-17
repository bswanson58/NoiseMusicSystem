using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Models;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using Noise.UI.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using ReusableBits.Interfaces;
using ReusableBits.Mvvm.VersionSpinner;
using ReusableBits.Support;
using ReusableBits.Ui.Platform;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IPrefixedNameHandler, PrefixedNameHandler>();
			containerRegistry.RegisterSingleton<ISelectionState, SelectionStateModel>();
            containerRegistry.Register<IPlayingItemHandler, PlayingItemHandler>();
			containerRegistry.Register<IUiLog, UiLogger>();
            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();

            containerRegistry.Register<IPlatformDialogService, PlatformDialogService>();

            containerRegistry.RegisterDialog<AlbumArtworkView, AlbumArtworkViewModel>();
            containerRegistry.RegisterDialog<AlbumEditDialog, AlbumEditDialogModel>();
            containerRegistry.RegisterDialog<ArtistArtworkView, ArtistArtworkViewModel>();
            containerRegistry.RegisterDialog<ArtistEditDialog, ArtistEditDialogModel>();
            containerRegistry.RegisterDialog<BulkTrackRenameView, BulkTrackRenameViewModel>();
            containerRegistry.RegisterDialog<ConfigurationDialog, ConfigurationViewModel>();
            containerRegistry.RegisterDialog<ExhaustedPlayPickerView, ExhaustedPlayPickerViewModel>();
            containerRegistry.RegisterDialog<LibraryBackupDialog, LibraryBackupDialogModel>();
            containerRegistry.RegisterDialog<LibraryConfigurationDialog, LibraryConfigurationDialogModel>();
            containerRegistry.RegisterDialog<PlaybackContextDialog, PlaybackContextDialogManager>();
            containerRegistry.RegisterDialog<PlayStrategyDialog, PlayStrategyDialogModel>();
            containerRegistry.RegisterDialog<TagAssociationDialog, TagAssociationDialogModel>();
            containerRegistry.RegisterDialog<TagAddDialog, TagEditDialogModel>();
            containerRegistry.RegisterDialog<TagEditDialog, TagEditDialogModel>();
            containerRegistry.RegisterDialog<TrackPlayPointsDialog, TrackPlayPointsDialogModel>();
            containerRegistry.RegisterDialog<TrackStrategyOptionsDialog, TrackStrategyOptionsDialogModel>();

            ViewModelLocationProvider.Register<PlaybackContextDialog, PlaybackContextDialogManager>();
            ViewModelLocationProvider.Register<PlayingTrackView, PlayerViewModel>();
            ViewModelLocationProvider.Register<PlayerExtendedView, PlayerViewModel>();
            ViewModelLocationProvider.Register<SmallPlayerView, PlayerViewModel>();
            ViewModelLocationProvider.Register<TagAddDialog, TagEditDialogModel>();
            ViewModelLocationProvider.Register<TrackPlayPointsDialog, TrackPlayPointsDialogModel>();

            // View registrations to improve startup times
            ViewModelLocationProvider.Register<AlbumArtworkView, AlbumArtworkViewModel>();
            ViewModelLocationProvider.Register<AlbumListView, AlbumListViewModel>();
            ViewModelLocationProvider.Register<AlbumTracksView, AlbumTracksViewModel>();
            ViewModelLocationProvider.Register<AlbumView, AlbumViewModel>();
            ViewModelLocationProvider.Register<ArtistArtworkView, ArtistArtworkViewModel>();
            ViewModelLocationProvider.Register<ArtistInfoView, ArtistInfoViewModel>();
            ViewModelLocationProvider.Register<ArtistListView, ArtistListViewModel>();
            ViewModelLocationProvider.Register<ArtistTracksView, ArtistTracksViewModel>();
            ViewModelLocationProvider.Register<ArtistView, ArtistViewModel>();
            ViewModelLocationProvider.Register<FavoritesView, FavoritesViewModel>();
            ViewModelLocationProvider.Register<LibraryAdditionsView, LibraryAdditionsViewModel>();
            ViewModelLocationProvider.Register<LibrarySelectorView, LibrarySelectorViewModel>();
            ViewModelLocationProvider.Register<LibraryView, LibraryViewModel>();
            ViewModelLocationProvider.Register<PlaybackRelatedView, PlaybackRelatedViewModel>();
            ViewModelLocationProvider.Register<PlayHistoryView, PlayHistoryViewModel>();
            ViewModelLocationProvider.Register<PlayingAlbumView, PlayingAlbumViewModel>();
            ViewModelLocationProvider.Register<PlayingArtistView, PlayingArtistViewModel>();
            ViewModelLocationProvider.Register<PlayingTransportView, PlayingTransportViewModel>();
            ViewModelLocationProvider.Register<PlayQueueControlView, PlayQueueControlViewModel>();
            ViewModelLocationProvider.Register<PlayQueueListView, PlayQueueListViewModel>();
            ViewModelLocationProvider.Register<PlayQueueStrategyView, PlayQueueStrategyViewModel>();
            ViewModelLocationProvider.Register<RatedTracksView, RatedTracksViewModel>();
            ViewModelLocationProvider.Register<RecentArtistView, RecentArtistViewModel>();
            ViewModelLocationProvider.Register<SearchView, SearchViewModel>();
            ViewModelLocationProvider.Register<StartupLibraryCreationView, StartupLibraryCreationViewModel>();
            ViewModelLocationProvider.Register<StartupLibrarySelectionView, StartupLibrarySelectionViewModel>();
            ViewModelLocationProvider.Register<StatusView, StatusViewModel>();
            ViewModelLocationProvider.Register<TagAssociationView, TagAssociationViewModel>();
            ViewModelLocationProvider.Register<TagListView, TagListViewModel>();
            ViewModelLocationProvider.Register<TimeExplorerAlbumsView, TimeExplorerAlbumsViewModel>();
            ViewModelLocationProvider.Register<TimeExplorerView, TimeExplorerViewModel>();
            ViewModelLocationProvider.Register<TimeExplorerYearView, TimeExplorerYearViewModel>();
            ViewModelLocationProvider.Register<TimelineView, TimelineViewModel>();

            var resourceLoader = new ResourceProvider( "Noise.UI", "Resources" );
            containerRegistry.RegisterInstance<IResourceProvider>( resourceLoader );
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( LibraryArtistView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( LibraryRecentView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( FavoritesView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( TagsView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( SearchView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( PlaybackRelatedView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryLeftPanel, typeof( LibraryAdditionsView ));

            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( ArtistInfoView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( AlbumInfoView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( ArtistTracksView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryAlbumPanel, typeof( RatedTracksView ));

            regionManager.RegisterViewWithRegion( RegionNames.LibraryRightPanel, typeof( PlayQueueView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryRightPanel, typeof( PlayHistoryView ));

            regionManager.RegisterViewWithRegion( RegionNames.LibraryPlayerPanel, typeof( PlayerView ));
            regionManager.RegisterViewWithRegion( RegionNames.LibraryPlayerPanel, typeof(PlayerExtendedView ));

            regionManager.RegisterViewWithRegion( RegionNames.TimelinePlayerPanel, typeof( PlayerView ));
            regionManager.RegisterViewWithRegion( RegionNames.TimelinePlayerPanel, typeof(PlayerExtendedView ));
        }
    }
}
