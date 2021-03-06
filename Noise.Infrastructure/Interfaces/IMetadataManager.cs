﻿using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum eMetadataType {
		ArtistArtwork,
		BandMembers,
		Biography,
		Genre,
		SimilarArtists,
		TopAlbums,
		TopTracks,
		WebSite,
		ActiveYears,
		Unknown
	}

	public interface IArtistMetadata {
		string		ArtistName { get; }

		IEnumerable<string> GetMetadataArray( eMetadataType metadataType );
		string				GetMetadata( eMetadataType metadataType );
	}

	public interface IArtistDiscography {
		string					    ArtistName { get; }
		List<DbDiscographyRelease>	Discography { get; } 
	}

	public interface IMetadataManager {
        IArtistMetadata		    GetArtistMetadata( string forArtist );
		IArtistDiscography	    GetArtistDiscography( string forArtist );

        bool                    ArtistPortfolioAvailable( string forArtist );
        IEnumerable<Artwork>    GetArtistPortfolio( string forArtist );
        Artwork				    GetArtistArtwork( string forArtist );

		void				    ExportMetadata( string exportPath );
		void				    ImportMetadata( string importPath );
	}
}
