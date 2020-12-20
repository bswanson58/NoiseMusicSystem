using System.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class TrackListViewModel : BindableBase {
        private readonly ITrackProvider         mTrackProvider;
        private readonly IClientState           mClientState;
        private readonly IQueuePlayProvider     mPlayProvider;
        private AlbumInfo                       mCurrentAlbum;
        private ObservableCollectionExtended<UiTrack>   mTrackList;

        public  string                          ArtistName { get; private set; }
        public  string                          AlbumName { get; private set; }

        public TrackListViewModel( ITrackProvider trackProvider, IQueuePlayProvider queuePlayProvider, IClientState clientState ) {
            mTrackProvider = trackProvider;
            mPlayProvider = queuePlayProvider;
            mClientState = clientState;
        }

        public ObservableCollectionExtended<UiTrack> DisplayList {
            get {
                if( mTrackList == null ) {
                    mTrackList = new ObservableCollectionExtended<UiTrack>();

                    Initialize();
                }

                return mTrackList;
            }
        }

        private void Initialize() {
            mCurrentAlbum = mClientState.CurrentAlbum;

            if( mCurrentAlbum != null ) {
                ArtistName = mCurrentAlbum.ArtistName;
                AlbumName = mCurrentAlbum.AlbumName;

                RaisePropertyChanged( nameof( ArtistName ));
                RaisePropertyChanged( nameof( AlbumName ));
            }

            LoadAlbums();
        }

        private async void LoadAlbums() {
            if( mCurrentAlbum != null ) {
                var list = await mTrackProvider.GetTrackList( mCurrentAlbum.ArtistId, mCurrentAlbum.AlbumId );

                if( list?.Success == true ) {
                    mTrackList.AddRange( from track in list.TrackList orderby track.VolumeName, track.TrackNumber select new UiTrack( track, OnPlay ));
                }
            }
        }

        private void OnPlay( UiTrack track ) {
            mPlayProvider.Queue( track.Track );
        }
    }
}
