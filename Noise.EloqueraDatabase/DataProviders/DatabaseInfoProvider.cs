using System;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	public class DatabaseInfoProvider : IDatabaseInfo {
		private readonly IEloqueraManager	mDatabaseManager;

		public DatabaseInfoProvider( IEloqueraManager databaseManager ) {
			mDatabaseManager = databaseManager;
		}

		public long DatabaseId {
			get {
				IDatabase	database = null;
				var			databaseId = Constants.cDatabaseNullOid;

				try {
					database = mDatabaseManager.ReserveDatabase();
					databaseId = database.DatabaseVersion.DatabaseId;
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "DatabaseInfoProvider:DatabaseId", ex );
				}
				finally {
					if( database != null ) {
						mDatabaseManager.FreeDatabase( database );
					}
				}

				return( databaseId );
			}
		}
	}
}
