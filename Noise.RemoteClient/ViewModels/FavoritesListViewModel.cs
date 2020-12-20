using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class FavoritesListViewModel : ListBase<UiTrack> {
        private readonly ITrackProvider     mTrackProvider;
        private PlayingState                mPlayingState;
        private IDisposable                 mPlayingStateSubscription;

        public FavoritesListViewModel( ITrackProvider trackProvider, IQueuePlayProvider queuePlayProvider, IHostInformationProvider hostInformationProvider,
                                       IClientState clientState ) :
            base( queuePlayProvider, hostInformationProvider ) {
            mTrackProvider = trackProvider;

            InitializeLibrarySubscription();
            mPlayingStateSubscription = clientState.CurrentlyPlaying.Subscribe( OnPlayingState );
        }

        protected override void OnLibraryStatusChanged( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                LoadList();
            }
        }

        private void OnPlayingState( PlayingState state ) {
            mPlayingState = state;

            DisplayList.ForEach( f => f.SetIsPlaying( mPlayingState ));
        }

        protected override async Task<IEnumerable<UiTrack>> RetrieveList() {
            IEnumerable<UiTrack> retValue = new List<UiTrack>();

            var list = await mTrackProvider.GetFavoriteTracks();

            if( list?.Success == true ) {
                retValue = from track in list.TrackList orderby track.TrackName, track.ArtistName, track.AlbumName 
                            select new UiTrack( track, OnPlay, mPlayingState );
            }

            return retValue;
        }

        public override void Dispose() {
            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;

            base.Dispose();
        }
    }
}
