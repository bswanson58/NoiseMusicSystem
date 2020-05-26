using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    enum PlayerViews {
        Regular,
        Extended
    }

    class LibraryViewModel : PropertyChangeBase, IHandle<Events.ViewDisplayRequest>, IHandle<Events.ExtendedPlayerRequest>, IHandle<Events.StandardPlayerRequest> {
        private readonly ArtistTracksViewModel  mArtistTracksViewModel;
        private readonly ArtistInfoViewModel    mArtistInfoViewModel;
        private readonly RatedTracksViewModel   mRatedTracksViewModel;

        public  PlayerViews                     CurrentPlayer { get; private set; }
        public  PropertyChangeBase              CurrentAlbumView {  get; private set; }

        public LibraryViewModel( IEventAggregator eventAggregator, ArtistInfoViewModel artistInfoView, 
                                 ArtistTracksViewModel artistTracksView, RatedTracksViewModel ratedTracksView ) {
            mArtistInfoViewModel = artistInfoView;
            mArtistTracksViewModel = artistTracksView;
            mRatedTracksViewModel = ratedTracksView;

            CurrentPlayer = PlayerViews.Regular;
            SetAlbumInfoView( mArtistInfoViewModel );

            eventAggregator.Subscribe( this );
        }

        public void Handle( Events.ViewDisplayRequest eventArgs ) {
            switch( eventArgs.ViewName ) {
                case ViewNames.AlbumInfoView:
                    break;

                case ViewNames.ArtistInfoView:
                    SetAlbumInfoView( mArtistInfoViewModel );
                    break;

                case ViewNames.ArtistTracksView:
                    SetAlbumInfoView( mArtistTracksViewModel );
                    break;

                case ViewNames.RatedTracksView:
                    SetAlbumInfoView( mRatedTracksViewModel );
                    break;

                case ViewNames.RelatedTracksView:
                    break;
            }
        }

        private void SetAlbumInfoView( PropertyChangeBase view ) {
            if( CurrentAlbumView is IActiveAware previousAware ) {
                previousAware.IsActive = false;
            }

            CurrentAlbumView = view;
            RaisePropertyChanged( () => CurrentAlbumView );

            if( CurrentAlbumView is IActiveAware currentAware ) {
                currentAware.IsActive = false;
            }
        }

        public void Handle( Events.StandardPlayerRequest eventArgs ) {
            SetPlayerView( PlayerViews.Regular );
        }

        public void Handle( Events.ExtendedPlayerRequest eventArgs ) {
            SetPlayerView( PlayerViews.Extended );
        }

        private void SetPlayerView( PlayerViews toPlayer ) {
            CurrentPlayer = toPlayer;

            RaisePropertyChanged( () => CurrentPlayer );
        }
    }
}
