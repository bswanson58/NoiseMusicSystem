using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class TrackListViewModel : BindableBase, IDisposable {
        private readonly ITrackProvider         mTrackProvider;
        private readonly IQueuePlayProvider     mQueuePlay;
        private IDisposable                     mLibraryStatusSubscription;
        private IDisposable                     mStateSubscription;
        private bool                            mLibraryOpen;
        private AlbumInfo                       mCurrentAlbum;

        public  string                          ArtistName { get; private set; }
        public  string                          AlbumName { get; private set; }
        public  ObservableCollection<UiTrack>   TrackList { get; }

        public TrackListViewModel( ITrackProvider trackProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider, 
                                   IClientState clientState ) {
            mTrackProvider = trackProvider;
            mQueuePlay = queuePlayProvider;

            TrackList = new ObservableCollection<UiTrack>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
            mStateSubscription = clientState.CurrentAlbum.Subscribe( OnAlbumState );
        }

        private void OnAlbumState( AlbumInfo album ) {
            mCurrentAlbum = album;

            if( mCurrentAlbum != null ) {
                ArtistName = mCurrentAlbum.ArtistName;
                AlbumName = mCurrentAlbum.AlbumName;

                RaisePropertyChanged( nameof( ArtistName ));
                RaisePropertyChanged( nameof( AlbumName ));
            }

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
                        TrackList.Add( new UiTrack( track, OnTrackPlay ));
                    }
                }
            }
        }

        private void OnTrackPlay( UiTrack track ) {
            mQueuePlay.QueueTrack( track.Track );
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mStateSubscription?.Dispose();
            mStateSubscription = null;
        }
    }
}
