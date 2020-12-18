using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.ViewModels {
    class TrackListViewModel : ListBase<UiTrack> {
        private readonly ITrackProvider         mTrackProvider;
        private IDisposable                     mStateSubscription;
        private AlbumInfo                       mCurrentAlbum;

        public  string                          ArtistName { get; private set; }
        public  string                          AlbumName { get; private set; }

        public TrackListViewModel( ITrackProvider trackProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider, 
                                   IClientState clientState ) :
            base( queuePlayProvider, hostInformationProvider ){
            mTrackProvider = trackProvider;

            InitializeLibrarySubscription();
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

            LoadList();
        }

        protected override async Task<IEnumerable<UiTrack>> RetrieveList() {
            IEnumerable<UiTrack> retValue = new List<UiTrack>();
            
            if( mCurrentAlbum != null ) {
                var list = await mTrackProvider.GetTrackList( mCurrentAlbum.ArtistId, mCurrentAlbum.AlbumId );

                if( list?.Success == true ) {
                    retValue = from track in list.TrackList orderby track.VolumeName, track.TrackNumber select new UiTrack( track, OnPlay );
                }
            }

            return retValue;
        }

        public override void Dispose() {
            mStateSubscription?.Dispose();
            mStateSubscription = null;
        }
    }
}
