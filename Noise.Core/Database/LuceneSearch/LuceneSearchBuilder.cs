using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CuttingEdge.Conditions;
using Lucene.Net.Index;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database.LuceneSearch {
	public class LuceneIndexBuilder : ISearchBuilder {
		private readonly INoiseLog	mLog;
		private readonly long		mArtistId;
		private IndexWriter			mIndexWriter;

		public LuceneIndexBuilder( DbArtist artist, string indexLocation, bool createIndex, INoiseLog log ) {
			mLog = log;
			mArtistId = artist.DbId;

			var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( indexLocation ));
			var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 );

			mIndexWriter = new IndexWriter( directory, analyzer, createIndex, IndexWriter.MaxFieldLength.UNLIMITED );
		}

		public void WriteTimeStamp() {
			using( new SearchItemDetails( mIndexWriter, mArtistId, DateTime.Now )) { }
		}

		private SearchItemDetails AddSearchItem( eSearchItemType itemType ) {
			return( new SearchItemDetails( mIndexWriter, itemType, mArtistId ));
		}

		public void AddSearchItem( eSearchItemType itemType, string searchText ) {
			using( var searchItem = AddSearchItem( itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		public void AddSearchItem( eSearchItemType itemType, IEnumerable<string> searchList ) {
			foreach( var searchText in searchList ) {
				using( var searchItem = AddSearchItem( itemType )) {
					searchItem.AddSearchText( searchText );
				}
			}
		}

		private SearchItemDetails AddSearchItem( DbAlbum album, eSearchItemType itemType ) {
			Condition.Requires( album ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter, itemType, mArtistId, album ));
		}

		public void AddSearchItem( DbAlbum album, eSearchItemType itemType, string searchText ) {
			Condition.Requires( album ).IsNotNull();

			using( var searchItem = AddSearchItem( album, itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		private SearchItemDetails AddSearchItem( DbAlbum album, DbTrack track, eSearchItemType itemType ) {
			Condition.Requires( album ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter, itemType, mArtistId, album, track ));
		}

		public void AddSearchItem( DbAlbum album, DbTrack track, eSearchItemType itemType, string searchText ) {
			Condition.Requires( album ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			using( var searchItem = AddSearchItem( album, track, itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		public void DeleteArtistSearchItems() {
			mIndexWriter.DeleteDocuments( new Term( SearchItemFieldName.cArtistId, mArtistId.ToString( CultureInfo.InvariantCulture )));
		}

		public void Dispose() {
			if( mIndexWriter != null ) {
				EndIndexUpdate();
			}
		}

		private void EndIndexUpdate() {
			if( mIndexWriter != null ) {
				try {
					mIndexWriter.Optimize();
					mIndexWriter.Dispose();
					mIndexWriter = null;
				}
				catch( Exception ex ) {
					mLog.LogException( "Search provider cannot close IndexWriter", ex );
				}
			}
		}
	}

}
