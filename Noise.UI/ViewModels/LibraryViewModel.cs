using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Prism.Regions;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    class LibraryViewModel : PropertyChangeBase, IHandle<Events.ViewDisplayRequest>, IHandle<Events.ExtendedPlayerRequest>, IHandle<Events.StandardPlayerRequest> {
        private readonly IRegionManager mRegionManager;

        public LibraryViewModel( IEventAggregator eventAggregator, IRegionManager regionManager ) {
            mRegionManager = regionManager;

            SetAlbumInfoView( ViewNames.ArtistInfoView );
            SetPlayerView( ViewNames.PlayerView );

            eventAggregator.Subscribe( this );
        }

        public void Handle( Events.ViewDisplayRequest eventArgs ) {
            if( eventArgs.ViewName.Equals( ViewNames.RelatedTracksView )) {
                SetLibraryView( ViewNames.RelatedTracksView );
            }
            else {
                SetAlbumInfoView( eventArgs.ViewName );
            }
        }

        private void SetLibraryView( string viewName ) {
            var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == RegionNames.LibraryLeftPanel );

            if( region != null ) {
                Execute.OnUIThread( () => region.RequestNavigate( new Uri( viewName, UriKind.Relative)));
            }
        }

        private void SetAlbumInfoView( string viewName ) {
            var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == RegionNames.LibraryAlbumPanel );

            if( region != null ) {
                Execute.OnUIThread( () => region.RequestNavigate( new Uri( viewName, UriKind.Relative)));
            }
        }

        public void Handle( Events.StandardPlayerRequest eventArgs ) {
            SetPlayerView( ViewNames.PlayerView );
        }

        public void Handle( Events.ExtendedPlayerRequest eventArgs ) {
            SetPlayerView( ViewNames.ExtendedPlayerView );
        }

        private void SetPlayerView( string viewName ) {
            var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == RegionNames.LibraryPlayerPanel );

            if( region != null ) {
                Execute.OnUIThread( () => region.RequestNavigate( new Uri( viewName, UriKind.Relative)));
            }
        }
    }
}
