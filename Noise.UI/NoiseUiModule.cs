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

            var resourceLoader = new ResourceProvider( "Noise.UI", "Resources" );
            containerRegistry.RegisterInstance<IResourceProvider>( resourceLoader );

            containerRegistry.RegisterDialog<AlbumArtworkView, AlbumArtworkViewModel>();
            containerRegistry.RegisterDialog<AlbumEditDialog, AlbumEditDialogModel>();
            containerRegistry.RegisterDialog<ArtistArtworkView, ArtistArtworkViewModel>();
            containerRegistry.RegisterDialog<ArtistEditDialog, ArtistEditDialogModel>();
            containerRegistry.RegisterDialog<ConfigurationDialog, ConfigurationViewModel>();
            containerRegistry.RegisterDialog<PlayStrategyDialog, PlayStrategyDialogModel>();
            containerRegistry.RegisterDialog<TagAssociationDialog, TagAssociationDialogModel>();
            containerRegistry.RegisterDialog<TrackStrategyOptionsDialog, TrackStrategyOptionsDialogModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            ViewModelLocationProvider.Register<PlayingTrackView, PlayerViewModel>();
            ViewModelLocationProvider.Register<PlayerExtendedView, PlayerViewModel>();

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
        }
    }
}
