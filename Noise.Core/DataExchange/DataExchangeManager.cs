using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal enum eExchangeType {
		Favorites,
		Streams
	}

	internal class DataExchangeManager : IDataExchangeManager {
		private readonly IUnityContainer	mContainer;

		public DataExchangeManager( IUnityContainer container ) {
			mContainer = container;
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

		private IDataExport CreateExporter( eExchangeType exportType ) {
			IDataExport	retValue = null;

			switch( exportType ) {
				case eExchangeType.Favorites:
					retValue = new ExportFavorites( mContainer );
					break;

				case eExchangeType.Streams:
					retValue = new ExportStreams( mContainer );
					break;
			}

			return( retValue );
		}
	}
}
