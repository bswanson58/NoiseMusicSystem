using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Prism;
using Prism.Regions;

namespace Noise.UI.ViewModels {
    class TimelineViewModel : IHandle<Events.ExtendedPlayerRequest>, IHandle<Events.StandardPlayerRequest> {
        private readonly IRegionManager mRegionManager;

        public TimelineViewModel( IRegionManager regionManager, IEventAggregator eventAggregator ) {
            mRegionManager = regionManager;

            eventAggregator.Subscribe( this );
        }

        public void Handle( Events.StandardPlayerRequest eventArgs ) {
            SetLibraryPlayerView( ViewNames.PlayerView );
        }

        public void Handle( Events.ExtendedPlayerRequest eventArgs ) {
            SetLibraryPlayerView( ViewNames.ExtendedPlayerView );
        }

        private void SetLibraryPlayerView( string viewName ) {
            var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == RegionNames.TimelinePlayerPanel );

            if( region != null ) {
                Execute.OnUIThread( () => region.RequestNavigate( new Uri( viewName, UriKind.Relative)));
            }
        }
    }
}
