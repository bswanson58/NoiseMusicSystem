using System;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;

namespace Noise.RemoteClient.ViewModels {
    class ArtistListViewModel {
        private readonly IArtistProvider    mArtistProvider;
        private IDisposable                 mLibraryStatusSubscription;

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider ) {
            mArtistProvider = artistProvider;

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                LoadArtistList();
            }
        }

        private async void LoadArtistList() {
            var list = await mArtistProvider.GetArtistList();

            if( list?.Success == true ) {
                var count = list.ArtistList.Count;
            }
        }
    }
}
