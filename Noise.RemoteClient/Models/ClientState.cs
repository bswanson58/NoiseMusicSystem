using System;
using System.Reactive.Subjects;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Models {
    class ClientState : IClientState {
        private readonly BehaviorSubject<SuggestionState>   mSuggestionSubject;

        public  ArtistInfo                                  CurrentArtist { get; private set; }
        public  AlbumInfo                                   CurrentAlbum { get; private set; }

        public  IObservable<SuggestionState>                CurrentSuggestion => mSuggestionSubject;

        public ClientState() {
            mSuggestionSubject = new BehaviorSubject<SuggestionState>( null );
        }

        public void SetCurrentArtist( ArtistInfo artist ) {
            CurrentArtist = artist;
        }

        public void SetCurrentAlbum( AlbumInfo album ) {
            CurrentAlbum = album;
        }

        public void SetSuggestionState( UiQueuedTrack forTrack ) {
            if( forTrack != null ) {
                mSuggestionSubject.OnNext( new SuggestionState( forTrack ));
            }
        }
    }
}
