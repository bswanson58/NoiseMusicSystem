using System.IO;
using System.Xml.Linq;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal enum eExchangeType {
		Favorites,
		Streams
	}

	internal class DataExchangeManager : IDataExchangeManager {
		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IInternetStreamProvider	mStreamProvider;
		private readonly INoiseLog					mLog;

		public DataExchangeManager( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IInternetStreamProvider streamProvider,
									INoiseLog log ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStreamProvider = streamProvider;
			mLog = log;
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
					retValue = new ExportFavorites( mArtistProvider, mAlbumProvider, mTrackProvider, mLog );
					break;

				case eExchangeType.Streams:
					retValue = new ExportStreams( mStreamProvider, mLog );
					break;
			}

			return( retValue );
		}

		private IDataImport CreateImporter( eExchangeType importType ) {
			IDataImport	retValue = null;

			switch( importType ) {
				case eExchangeType.Favorites:
					retValue = new ImportFavorites( mArtistProvider, mAlbumProvider, mTrackProvider );
					break;

				case eExchangeType.Streams:
					retValue = new ImportStreams( mStreamProvider );
					break;
			}

			return( retValue );
		}
	}
}
