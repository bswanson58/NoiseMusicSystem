using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Navigation;

namespace Noise.RemoteClient.ViewModels {
    class TrackListViewModel : ViewModelBase {
        private readonly ITrackProvider mTrackProvider;
        private IDisposable             mLibraryStatusSubscription;
        private bool                    mLibraryOpen;
        private long                    mArtistId;
        private long                    mAlbumId;

        public  ObservableCollection<TrackInfo>    TrackList { get; }

        public TrackListViewModel( ITrackProvider trackProvider, IHostInformationProvider hostInformationProvider, INavigationService navigationService ) :
        base( navigationService ) {
            mTrackProvider = trackProvider;

            TrackList = new ObservableCollection<TrackInfo>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        public override void OnNavigatedTo( INavigationParameters parameters ) {
            base.OnNavigatedTo( parameters );
            mArtistId = parameters.GetValue<long>( NavigationKeys.ArtistId );
            mAlbumId = parameters.GetValue<long>( NavigationKeys.AlbumId );

            LoadTrackList( mArtistId, mAlbumId );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status.LibraryOpen;
        }

        private async void LoadTrackList( long artistId, long albumId ) {
            TrackList.Clear();

            if(( mLibraryOpen ) &&
               ( artistId != Constants.cNullId ) &&
               ( albumId != Constants.cNullId )) {
                var list = await mTrackProvider.GetTrackList( artistId, albumId );

                if( list?.Success == true ) {
                    foreach( var track in list.TrackList.OrderBy( a => a.TrackNumber )) {
                        TrackList.Add( track );
                    }
                }
            }
        }

        public override void Destroy() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;
        }
    }
}
