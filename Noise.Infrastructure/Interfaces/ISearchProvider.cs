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
		bool			Initialize();

		ISearchBuilder	CreateIndexBuilder( DbArtist forArtist, bool createIndex );
		DateTime		DetermineTimeStamp( DbArtist artist );

		IEnumerable<SearchResultItem>	Search( string queryText, int maxResults );
	}

	public interface ISearchBuilder : IDisposable {
		void		DeleteArtistSearchItems();
		void		WriteTimeStamp();

		void		AddSearchItem( eSearchItemType itemType, string searchText );
		void		AddSearchItem( eSearchItemType itemType, IEnumerable<string> searchList );
		void		AddSearchItem( DbAlbum album, eSearchItemType itemType, string searchText );
		void		AddSearchItem( DbAlbum album, DbTrack track, eSearchItemType itemType, string searchText );
	}
}
