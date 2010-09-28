using System;
using CuttingEdge.Conditions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class SearchItemDetails : ISearchItemDetails {
		private	readonly IndexWriter	mWriter;
		private Document				mDocument;

		internal SearchItemDetails( IndexWriter writer ) {
			mWriter = writer;
			mDocument = new Document();
		}

		public void AddIndex( string indexName, string indexText ) {
			mDocument.Add( new Field( indexName, indexText, Field.Store.YES, Field.Index.NOT_ANALYZED ));
		}

		public void AddSearchText( string detailName, string detail ) {
			mDocument.Add( new Field( detailName, detail, Field.Store.NO, Field.Index.ANALYZED ));
		}

		public void CommitItem() {
			Condition.Requires( mDocument ).IsNotNull();

			if( mDocument != null ) {
				mWriter.AddDocument( mDocument );
				
				mDocument = null;
			}
		}

		public void Dispose() {
			CommitItem();
		}
	}

	public class LuceneSearchProvider : ISearchProvider {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private bool						mIsInitialized;
		private	string						mIndexLocation;
		private IndexWriter					mIndexWriter;

		public LuceneSearchProvider( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public bool Initialize() {
			mIsInitialized = false;

			var configMgr = mContainer.Resolve<ISystemConfiguration>();
			var config = configMgr.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

			if( config != null ) {
				mIndexLocation = config.SearchIndexLocation;

				if( System.IO.Directory.Exists( mIndexLocation )) {
					mIsInitialized = true;
				}
			}
			else {
				mLog.LogMessage( "Database configuration could not be loaded." );
			}

			return( mIsInitialized );
		}

		public bool StartIndexUpdate( bool createIndex ) {
			var retValue = false;

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
					var directory = new Lucene.Net.Store.SimpleFSDirectory( new System.IO.DirectoryInfo( mIndexLocation ));
					var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 );

					mIndexWriter = new IndexWriter( directory, analyzer, createIndex, IndexWriter.MaxFieldLength.UNLIMITED );

					retValue = true;
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Search provider cannot open IndexWriter: ", ex );
				}
			}

			return( retValue );
		}

		public ISearchItemDetails AddSearchItem() {
			Condition.Requires( mIndexWriter ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter ));
		}

		public bool EndIndexUpdate() {
			var retValue = false;

			if( mIndexWriter != null ) {
				try {
					mIndexWriter.Optimize();
					mIndexWriter.Close();

					retValue = true;
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Search provider cannot close IndexWriter: ", ex );
				}
			}

			return( retValue );
		}
	}
}
