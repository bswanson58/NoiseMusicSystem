using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Metadata.Interfaces {
	public interface IArtistMetadataManager {
		void				    ArtistMentioned( string artistName );
		void				    ArtistForgotten( string artistName );

		IArtistMetadata		    GetArtistBiography( string artistName );
		IArtistDiscography	    GetArtistDiscography( string artistName );

        bool                    ArtistPortfolioAvailable( string forArtist );
		Artwork				    GetArtistArtwork( string forArtist );
        IEnumerable<Artwork>    GetArtistPortfolio( string forArtist );
	}
}
