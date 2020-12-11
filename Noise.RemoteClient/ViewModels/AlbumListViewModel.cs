using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Noise.RemoteClient.Views;
using Noise.RemoteServer.Protocol;
using Prism.Navigation;

namespace Noise.RemoteClient.ViewModels {
    class AlbumListViewModel : ViewModelBase {
        private readonly IAlbumProvider mAlbumProvider;
        private IDisposable             mLibraryStatusSubscription;
        private long                    mArtistId;
        private bool                    mLibraryOpen;
        private AlbumInfo               mSelectedAlbum;

        public  ObservableCollection<AlbumInfo> AlbumList { get; }

        public AlbumListViewModel( IAlbumProvider albumProvider, IHostInformationProvider hostInformationProvider, INavigationService navigationService ) :
        base( navigationService ) {
            mAlbumProvider = albumProvider;

            AlbumList = new ObservableCollection<AlbumInfo>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        public AlbumInfo SelectedAlbum {
            get => mSelectedAlbum;
            set => SetProperty( ref mSelectedAlbum, value, OnAlbumSelected );
        }

        private void OnAlbumSelected() {
            if( mSelectedAlbum != null ) {
                var navigationParameters = new NavigationParameters {
                    { NavigationKeys.ArtistId, mSelectedAlbum.ArtistId },
                    { NavigationKeys.AlbumId, mSelectedAlbum.AlbumId }};

                NavigationService.NavigateAsync( nameof( TrackList ), navigationParameters );
            }
        }

        public override void OnNavigatedTo( INavigationParameters parameters ) {
            if( parameters.GetNavigationMode() == NavigationMode.Back ) {
                SelectedAlbum = null;
            }
            else {
                mArtistId = parameters.GetValue<long>( NavigationKeys.ArtistId );

                LoadAlbumList( mArtistId );
            }
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status.LibraryOpen;
        }

        private async void LoadAlbumList( long artistId ) {
            AlbumList.Clear();

            if(( mLibraryOpen ) &&
               ( artistId != Constants.cNullId )) {
                var list = await mAlbumProvider.GetAlbumList( artistId );

                if( list?.Success == true ) {
                    foreach( var artist in list.AlbumList.OrderBy( a => a.AlbumName )) {
                        AlbumList.Add( artist );
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
