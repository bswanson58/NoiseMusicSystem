using System;

namespace Noise.Infrastructure.Interfaces {
	public interface ISearchItemDetails : IDisposable {
		void AddIndex( string indexName, string indexText );
		void AddSearchText( string detailName, string detail );

		void CommitItem();
	}

	public interface ISearchProvider {
		bool		Initialize();

		bool				StartIndexUpdate( bool createIndex );
		ISearchItemDetails	AddSearchItem();
		bool				EndIndexUpdate();
	}
}
