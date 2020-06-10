using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Prism;
using Prism.Regions;

namespace Noise.UI.ViewModels {
    class TimelineViewModel : IActiveAware, IHandle<Events.ExtendedPlayerRequest>, IHandle<Events.StandardPlayerRequest> {
        private readonly IRegionManager mRegionManager;
        private bool                    mIsActive;

        public  event EventHandler  IsActiveChanged = delegate { };

        public TimelineViewModel( IRegionManager regionManager, IEventAggregator eventAggregator ) {
            mRegionManager = regionManager;

            eventAggregator.Subscribe( this );
        }

        public bool IsActive {
            get => ( mIsActive );
            set {
                mIsActive = value;

                IsActiveChanged( this, new EventArgs());
            }
        }

        public void Handle( Events.StandardPlayerRequest eventArgs ) {
            SetLibraryPlayerView( ViewNames.PlayerView );
        }

        public void Handle( Events.ExtendedPlayerRequest eventArgs ) {
            SetLibraryPlayerView( ViewNames.ExtendedPlayerView );
        }

        private void SetLibraryPlayerView( string viewName ) {
            if( IsActive ) {
                var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == RegionNames.TimelinePlayerPanel );

                if( region != null ) {
                    Execute.OnUIThread( () => region.RequestNavigate( new Uri( viewName, UriKind.Relative)));
                }
            }
        }
    }
}
