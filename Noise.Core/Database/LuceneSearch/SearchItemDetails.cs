using System;
using System.Collections.Generic;
using System.Globalization;
using CuttingEdge.Conditions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database.LuceneSearch {
	internal class SearchItemFieldName {
		public	const string	cArtistId = "artistId";
		public	const string	cAlbumId = "albumId";
		public	const string	cTrackId = "trackId";
		public	const string	cItemType = "itemType";
		public	const string	cContent = "content";
		public	const string	cTimeStamp = "timeStamp";
	}

	internal class SearchItemDetails : IDisposable {
		private	IndexWriter		mWriter;
		private Document		mDocument;

		internal SearchItemDetails( IndexWriter writer, long artistId, DateTime timeStamp ) {
			mWriter = writer;
			mDocument = new Document();

			mDocument.Add( new Field( SearchItemFieldName.cItemType, eSearchItemType.TimeStamp.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cArtistId, artistId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cTimeStamp, timeStamp.Ticks.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NO ));
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, long artistId ) :
			this( writer, itemType, artistId, null, null ) {
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, long artistId, DbAlbum album ) :
			this( writer, itemType, artistId, album, null ) {
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, long artistId, DbAlbum album, DbTrack track ) {
			mWriter = writer;
			mDocument = new Document();

			mDocument.Add( new Field( SearchItemFieldName.cItemType, itemType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cArtistId, artistId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			if( album != null ) {
				mDocument.Add( new Field( SearchItemFieldName.cAlbumId, album.DbId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NO ));
			}
			if( track != null ) {
				mDocument.Add( new Field( SearchItemFieldName.cTrackId, track.DbId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NO ));
			}

			var	boost = 1.0f;
			switch( itemType ) {
				case eSearchItemType.Album:
					boost = 1.3f;
					break;

				case eSearchItemType.Artist:
					boost = 1.2f;
					break;

				case eSearchItemType.Track:
					boost = 1.1f;
					break;

				case eSearchItemType.BandMember:
				case eSearchItemType.Biography:
				case eSearchItemType.Lyrics:
				case eSearchItemType.TextInfo:
				case eSearchItemType.TopAlbum:
				case eSearchItemType.TimeStamp:
					break;

				case eSearchItemType.Discography:
				case eSearchItemType.SimilarArtist:
					boost = 0.8f;
					break;
			}

			mDocument.Boost = boost;
		}

		public void AddSearchText( string searchText ) {
			Condition.Requires( mDocument ).IsNotNull();

			mDocument.Add( new Field( SearchItemFieldName.cContent, searchText, Field.Store.NO, Field.Index.ANALYZED ));
		}

		public void AddSearchText( IEnumerable<string> searchText ) {
			foreach( var text in searchText ) {
				AddSearchText( text );
			}
		}

		public void CommitItem() {
			Condition.Requires( mDocument ).IsNotNull();

			if(( mDocument != null ) &&
			   ( mWriter != null )) {
				mWriter.AddDocument( mDocument );
				
				mDocument = null;
				mWriter = null;
			}
		}

		public void Dispose() {
			CommitItem();
		}
	}

}
