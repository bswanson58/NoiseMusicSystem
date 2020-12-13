using System;
using Noise.RemoteClient.Dto;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IClientState {
        IObservable<ArtistInfo>         CurrentArtist { get; }
        IObservable<AlbumInfo>          CurrentAlbum { get; }
        IObservable<SuggestionState>    CurrentSuggestion { get; }

        void                            SetCurrentArtist( ArtistInfo artist );
        void                            SetCurrentAlbum( AlbumInfo album );
        void                            SetSuggestionState( UiQueuedTrack forTrack );
    }
}
