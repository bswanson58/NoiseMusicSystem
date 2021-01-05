using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class TrackSuggestionsViewModel : ListBase<UiTrack> {
        private readonly ITrackProvider                 mTrackProvider;
        private PlayingState                            mPlayingState;
        private SuggestionState                         mSuggestionState;
        private IDisposable                             mClientStateSubscription;
        private IDisposable                             mPlayingStateSubscription;

        public TrackSuggestionsViewModel( ITrackProvider trackProvider, IQueuePlayProvider playProvider, IClientState clientState, IQueueListener queueListener,
                                          IHostInformationProvider hostInformationProvider ) :
            base( playProvider, hostInformationProvider ) {
            mTrackProvider = trackProvider;

            InitializeLibrarySubscription();
            mPlayingStateSubscription = queueListener.CurrentlyPlaying.Subscribe( OnPlaying );
            mClientStateSubscription = clientState.CurrentSuggestion.Subscribe( OnSuggestion );
        }

        private void OnSuggestion( SuggestionState state ) {
            mSuggestionState = state;

            LoadList();
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            DisplayList.ForEach( t => t.SetIsPlaying( mPlayingState ));
        }

        protected override async Task<IEnumerable<UiTrack>> RetrieveList() {
            IEnumerable<UiTrack> retValue = new List<UiTrack>();

            if( mSuggestionState != null ) {
                var list = await mTrackProvider.GetSimilarTracks( mSuggestionState.TrackId );

                if( list?.Success == true ) {
                    retValue = from track in list.TrackList 
                        orderby track.GetRatingSort() descending, track.TrackName, track.ArtistName, track.AlbumName 
                        select new UiTrack( track, OnPlay, mPlayingState );
                }
            }

            return retValue;
        }

        public override void Dispose() {
            mClientStateSubscription?.Dispose();
            mClientStateSubscription = null;

            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;

            base.Dispose();
        }
    }
}
