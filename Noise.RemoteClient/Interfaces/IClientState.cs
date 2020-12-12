using System;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IClientState {
        IObservable<ArtistInfo>     CurrentArtist { get; }
        IObservable<AlbumInfo>      CurrentAlbum { get; }

        void                        SetCurrentArtist( ArtistInfo artist );
        void                        SetCurrentAlbum( AlbumInfo album );
    }
}
