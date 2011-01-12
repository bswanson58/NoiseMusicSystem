using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum eSearchItemType {
		Artist,
		Album,
		Track,
		BandMember,
		Biography,
		Discography,
		SimilarArtist,
		TopAlbum,
		TextInfo,
		TimeStamp,
		Unknown
	}

	public interface ISearchProvider {
		bool		Initialize();

		bool		StartIndexUpdate( bool createIndex );
		void		DeleteArtistSearchItems( DbArtist artist );
		void		WriteTimeStamp( DbArtist artist );

		void		AddSearchItem( DbArtist artist, eSearchItemType itemType, string searchText );
		void		AddSearchItem( DbArtist artist, eSearchItemType itemType, IEnumerable<string> searchList );
		void		AddSearchItem( DbArtist artist, DbAlbum album, eSearchItemType itemType, string searchText );
		void		AddSearchItem( DbArtist artist, DbAlbum album, DbTrack track, eSearchItemType itemType, string searchText );
		bool		EndIndexUpdate();

		DateTime	DetermineTimeStamp( DbArtist artist );

		IEnumerable<SearchResultItem>	Search( string queryText );
	}
}
