using Noise.Infrastructure.Dto;

namespace Noise.Metadata.Interfaces {
    interface IArtistArtworkSelector {
        void    SelectArtwork( string artistName, Artwork forArtist );
    }
}
