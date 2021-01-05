using System;
using Noise.RemoteClient.Dto;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IClientState {
        IObservable<SuggestionState>    CurrentSuggestion { get; }

        ArtistInfo                      CurrentArtist { get; }
        AlbumInfo                       CurrentAlbum { get; }

        void                            SetCurrentArtist( ArtistInfo artist );
        void                            SetCurrentAlbum( AlbumInfo album );
        void                            SetSuggestionState( UiQueuedTrack forTrack );
    }
}
