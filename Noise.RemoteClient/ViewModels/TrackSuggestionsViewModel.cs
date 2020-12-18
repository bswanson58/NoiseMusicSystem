using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;

namespace Noise.RemoteClient.ViewModels {
    class TrackSuggestionsViewModel : ListBase<UiTrack> {
        private readonly ITrackProvider                 mTrackProvider;
        private SuggestionState                         mSuggestionState;
        private IDisposable                             mClientStateSubscription;

        public TrackSuggestionsViewModel( ITrackProvider trackProvider, IQueuePlayProvider playProvider, IClientState clientState,
                                          IHostInformationProvider hostInformationProvider ) :
            base( playProvider, hostInformationProvider ) {
            mTrackProvider = trackProvider;

            InitializeLibrarySubscription();
            mClientStateSubscription = clientState.CurrentSuggestion.Subscribe( OnSuggestion );
        }

        private void OnSuggestion( SuggestionState state ) {
            mSuggestionState = state;

            LoadList();
        }

        protected override async Task<IEnumerable<UiTrack>> RetrieveList() {
            IEnumerable<UiTrack> retValue = new List<UiTrack>();

            if( mSuggestionState != null ) {
                var list = await mTrackProvider.GetSimilarTracks( mSuggestionState.TrackId );

                if( list?.Success == true ) {
                    retValue = from track in list.TrackList 
                        orderby track.GetRatingSort() descending, track.TrackName, track.ArtistName, track.AlbumName 
                        select new UiTrack( track, OnPlay );
                }
            }

            return retValue;
        }

        public override void Dispose() {
            mClientStateSubscription?.Dispose();
            mClientStateSubscription = null;
        }
    }
}
