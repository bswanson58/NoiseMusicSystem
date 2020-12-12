using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class TrackListViewModel : BindableBase, IDisposable {
        private readonly ITrackProvider mTrackProvider;
        private readonly IClientState   mClientState;
        private IDisposable             mLibraryStatusSubscription;
        private IDisposable             mStateSubscription;
        private bool                    mLibraryOpen;
        private AlbumInfo               mCurrentAlbum;

        public  ObservableCollection<TrackInfo>    TrackList { get; }

        public TrackListViewModel( ITrackProvider trackProvider, IHostInformationProvider hostInformationProvider, IClientState clientState ) {
            mTrackProvider = trackProvider;
            mClientState = clientState;

            TrackList = new ObservableCollection<TrackInfo>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
            mStateSubscription = mClientState.CurrentAlbum.Subscribe( OnAlbumState );
        }

        private void OnAlbumState( AlbumInfo album ) {
            mCurrentAlbum = album;

            LoadTrackList();
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status.LibraryOpen;

            LoadTrackList();
        }

        private async void LoadTrackList() {
            TrackList.Clear();

            if(( mLibraryOpen ) &&
               ( mCurrentAlbum != null )) {
                var list = await mTrackProvider.GetTrackList( mCurrentAlbum.ArtistId, mCurrentAlbum.AlbumId );

                if( list?.Success == true ) {
                    foreach( var track in list.TrackList.OrderBy( a => a.TrackNumber )) {
                        TrackList.Add( track );
                    }
                }
            }
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mStateSubscription?.Dispose();
            mStateSubscription = null;
        }
    }
}
