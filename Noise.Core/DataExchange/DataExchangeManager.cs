using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal enum eExchangeType {
		Favorites
	}

	internal class DataExchangeManager : IDataExchangeManager {
		private readonly IUnityContainer	mContainer;

		public DataExchangeManager( IUnityContainer container ) {
			mContainer = container;
		}

		public bool ExportFavorites( string fileName ) {
			var retValue = false;
			var	exporter = CreateExporter( eExchangeType.Favorites );

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
			}

			return( retValue );
		}
	}
}
