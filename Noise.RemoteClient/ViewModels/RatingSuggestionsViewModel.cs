using System;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class RatingSuggestionsViewModel : BindableBase {
        private readonly ITrackProvider     mTrackProvider;
        private readonly IQueuePlayProvider mQueuePlay;
        private SuggestionState             mSuggestionState;
        private IDisposable                 mClientStateSubscription;

        public  ObservableCollection<UiTrack>   TrackList { get; }

        public RatingSuggestionsViewModel( ITrackProvider trackProvider, IQueuePlayProvider playProvider, IClientState clientState ) {
            mTrackProvider = trackProvider;
            mQueuePlay = playProvider;

            TrackList = new ObservableCollection<UiTrack>();

            mClientStateSubscription = clientState.CurrentSuggestion.Subscribe( OnSuggestion );
        }

        private void OnSuggestion( SuggestionState state ) {
            mSuggestionState = state;

            LoadTracks();
        }

        private async void LoadTracks() {
            TrackList.Clear();

            if( mSuggestionState != null ) {
                var list = await mTrackProvider.GetRatedTracks( mSuggestionState.ArtistId, 3, true );

                if( list?.Success == true ) {
                    foreach( var track in list.TrackList.OrderBy( a => a.TrackNumber )) {
                        TrackList.Add( new UiTrack( track, OnTrackPlay ));
                    }
                }
            }
        }

        private void OnTrackPlay( UiTrack track) {
            mQueuePlay.QueueTrack( track.Track );
        }

        public void Dispose() {
            mClientStateSubscription?.Dispose();
            mClientStateSubscription = null;
        }
    }
}
