using System.IO;
using System.Xml.Linq;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal enum eExchangeType {
		Favorites,
		Streams
	}

	internal class DataExchangeManager : IDataExchangeManager {
		private readonly	IDataProvider	mDataProvider;

		public DataExchangeManager( IDataProvider dataProvider ) {
			mDataProvider = dataProvider;
		}

		public bool ExportFavorites( string fileName ) {
			return( Export( eExchangeType.Favorites, fileName ));
		}

		public bool ExportStreams( string fileName ) {
			return( Export( eExchangeType.Streams, fileName ));
		}

		private bool Export( eExchangeType exportType, string fileName ) {
			var retValue = false;
			var	exporter = CreateExporter( exportType );

			if( exporter != null ) {
				retValue = exporter.Export( fileName );
			}

			return( retValue );
		}

		public int Import( string fileName, bool eliminateDuplicates ) {
			var retValue = 0;

			if((!string.IsNullOrWhiteSpace( fileName )) &&
			   ( File.Exists( fileName ))) {
				var importDoc = XDocument.Load( fileName );

				foreach( var listNode in importDoc.Elements()) {
					if( listNode.Name.LocalName.Equals( ExchangeConstants.cStreamList )) {
						retValue += Import( eExchangeType.Streams, listNode, eliminateDuplicates  );
					}
					else if( listNode.Name.LocalName.Equals( ExchangeConstants.cFavoriteList )) {
						retValue += Import( eExchangeType.Favorites, listNode, eliminateDuplicates  );
					}
				}
			}

			return( retValue );
		}

		private int Import( eExchangeType importType, XElement rootNode, bool eliminateDuplicates ) {
			var retValue = 0;
			var importer = CreateImporter( importType );

			if( importer != null ) {
				retValue = importer.Import( rootNode, eliminateDuplicates );
			}

			return( retValue );
		}

		private IDataExport CreateExporter( eExchangeType exportType ) {
			IDataExport	retValue = null;

			switch( exportType ) {
				case eExchangeType.Favorites:
					retValue = new ExportFavorites( mDataProvider );
					break;

				case eExchangeType.Streams:
					retValue = new ExportStreams( mDataProvider );
					break;
			}

			return( retValue );
		}

		private IDataImport CreateImporter( eExchangeType importType ) {
			IDataImport	retValue = null;

			switch( importType ) {
				case eExchangeType.Favorites:
					retValue = new ImportFavorites( mDataProvider );
					break;

				case eExchangeType.Streams:
					retValue = new ImportStreams( mDataProvider );
					break;
			}

			return( retValue );
		}
	}
}
