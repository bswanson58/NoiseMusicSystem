using System;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class SuggestionsViewModel : BindableBase, IDisposable {
        private SuggestionState     mSuggestionState;
        private IDisposable         mClientStateSubscription;

        public  string              ArtistName { get; private set; }
        public  string              AlbumName { get; private set; }
        public  string              TrackName { get; private set; }

        public SuggestionsViewModel( IClientState clientState ) {
            mClientStateSubscription = clientState.CurrentSuggestion.Subscribe( OnSuggestion );
        }

        private void OnSuggestion( SuggestionState state ) {
            mSuggestionState = state;

            if( mSuggestionState != null ) {
                ArtistName = mSuggestionState.ArtistName;
                AlbumName = mSuggestionState.AlbumName;
                TrackName = mSuggestionState.TrackName;

                RaisePropertyChanged( nameof( ArtistName ));
                RaisePropertyChanged( nameof( AlbumName ));
                RaisePropertyChanged( nameof( TrackName ));
            }
        }

        public void Dispose() {
            mClientStateSubscription?.Dispose();
            mClientStateSubscription = null;
        }
    }
}
