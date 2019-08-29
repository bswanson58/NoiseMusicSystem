using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Metadata.Interfaces {
    interface IArtistArtworkSelector {
        bool                    ArtistPortfolioAvailable( string forArtist );
        
        void                    SelectArtwork( string artistName, Artwork forArtist );
        IEnumerable<Artwork>    ArtworkPortfolio( string artistName );
    }
}
