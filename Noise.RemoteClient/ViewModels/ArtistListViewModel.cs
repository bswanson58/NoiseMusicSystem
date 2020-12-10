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
    class ArtistListViewModel : ViewModelBase {
        private readonly IArtistProvider    mArtistProvider;
        private IDisposable                 mLibraryStatusSubscription;
        private ArtistInfo                  mSelectedArtist;

        public  ObservableCollection<ArtistInfo>    ArtistList { get; }

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider, INavigationService navigationService ) :
            base( navigationService ) {
            mArtistProvider = artistProvider;

            ArtistList = new ObservableCollection<ArtistInfo>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        public ArtistInfo SelectedArtist {
            get => mSelectedArtist;
            set => SetProperty( ref mSelectedArtist, value, OnArtistSelected );
        }

        private void OnArtistSelected() {
            var navigationParameters = new NavigationParameters { { NavigationKeys.ArtistId, mSelectedArtist.DbId }};

            NavigationService.NavigateAsync( nameof( AlbumList ), navigationParameters );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                LoadArtistList();
            }
            else {
                ArtistList.Clear();
            }
        }

        private async void LoadArtistList() {
            ArtistList.Clear();

            var list = await mArtistProvider.GetArtistList();

            if( list?.Success == true ) {
                foreach( var artist in list.ArtistList.OrderBy( a => a.ArtistName )) {
                    ArtistList.Add( artist );
                }
            }
        }

        public override void Destroy() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;
        }
    }
}
