using System;
using System.Collections.Generic;
using DynamicData;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum eSearchItemType {
		Artist,
		Album,
		Track,
		BandMember,
		Biography,
		Discography,
		Lyrics,
		SimilarArtist,
		TopAlbum,
		TextInfo,
		TimeStamp,
		Unknown,
		Everything
	}

	public interface ISearchProvider {
		bool			Initialize();

		ISearchBuilder	CreateIndexBuilder( DbArtist forArtist, bool createIndex );
		DateTime		DetermineTimeStamp( DbArtist artist );

        IEnumerable<SearchResultItem>				Search( eSearchItemType searchType, string queryText, int maxResults );
        
        IObservable<IChangeSet<SearchResultItem>>	SearchResults { get; }
		void										StartSearch( eSearchItemType searchType, string queryText );
		void										ClearSearch();
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
