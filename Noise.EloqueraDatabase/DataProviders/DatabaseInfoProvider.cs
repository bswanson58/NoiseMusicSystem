using System;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	public class DatabaseInfoProvider : IDatabaseInfo {
		private readonly long	mDatabaseId;

		public DatabaseInfoProvider( IEloqueraManager databaseManager ) {
			IDatabase	database = null;

			try {
				database = databaseManager.ReserveDatabase();
				mDatabaseId = database.DatabaseVersion.DatabaseId;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "DomainSearchProvider:DatabaseId", ex );
			}
			finally {
				if( database != null ) {
					databaseManager.FreeDatabase( database );
				}
			}
		}

		public long DatabaseId {
			get { return( mDatabaseId ); }
		}
	}
}
