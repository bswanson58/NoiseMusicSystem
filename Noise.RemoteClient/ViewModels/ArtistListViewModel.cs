using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.ViewModels {
    class ArtistListViewModel {
        private readonly IArtistProvider    mArtistProvider;
        private IDisposable                 mLibraryStatusSubscription;

        public  ObservableCollection<ArtistInfo>    ArtistList { get; }

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider ) {
            mArtistProvider = artistProvider;

            ArtistList = new ObservableCollection<ArtistInfo>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                LoadArtistList();
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
    }
}
