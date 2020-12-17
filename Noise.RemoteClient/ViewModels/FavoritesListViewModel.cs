using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class FavoritesListViewModel : BindableBase, IDisposable {
        private readonly ITrackProvider         mTrackProvider;
        private readonly IQueuePlayProvider     mQueuePlay;
        private IDisposable                     mLibraryStatusSubscription;
        private IDisposable                     mStateSubscription;
        private bool                            mLibraryOpen;

        public  ObservableCollection<UiTrack>   TrackList { get; }

        public FavoritesListViewModel( ITrackProvider trackProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider ) {
            mTrackProvider = trackProvider;
            mQueuePlay = queuePlayProvider;

            TrackList = new ObservableCollection<UiTrack>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status.LibraryOpen;

            LoadTrackList();
        }

        private async void LoadTrackList() {
            TrackList.Clear();

            if( mLibraryOpen ) {
                var list = await mTrackProvider.GetFavoriteTracks();

                if( list?.Success == true ) {
                    foreach( var track in list.TrackList.OrderBy( t => t.TrackName ).ThenBy( t => t.ArtistName ).ThenBy( t => t.AlbumName )) {
                        TrackList.Add( new UiTrack( track, OnTrackPlay ));
                    }
                }
            }
        }

        private void OnTrackPlay( UiTrack track ) {
            mQueuePlay.Queue( track.Track );
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mStateSubscription?.Dispose();
            mStateSubscription = null;
        }
    }
}
