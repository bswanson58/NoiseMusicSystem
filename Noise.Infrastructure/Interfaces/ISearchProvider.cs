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
		Unknown
	}

	public interface ISearchProvider {
		bool		Initialize();

		bool		StartIndexUpdate( bool createIndex );
		void		AddSearchItem( DbArtist artist, eSearchItemType itemType, string searchText );
		void		AddSearchItem( DbArtist artist, eSearchItemType itemType, IEnumerable<string> searchList );
		void		AddSearchItem( DbArtist artist, DbAlbum album, eSearchItemType itemTtpe, string searchText );
		bool		EndIndexUpdate();

		IEnumerable<SearchResultItem>	Search( string queryText );
	}
}
