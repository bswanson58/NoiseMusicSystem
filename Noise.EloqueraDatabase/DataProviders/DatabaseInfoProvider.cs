using System;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	public class DatabaseInfoProvider : IDatabaseInfo {
		private readonly IEloqueraManager	mDatabaseManager;
		private DbVersion					mDatabaseVersion;

		public DatabaseInfoProvider( IEloqueraManager databaseManager ) {
			mDatabaseManager = databaseManager;
		}

		public bool IsOpen {
			get{ return( mDatabaseManager.IsOpen ); }
		}

		public long DatabaseId {
			get {
				var	databaseId = Constants.cDatabaseNullOid;

				if( mDatabaseVersion == null ) {
					RetrieveDatabaseVerson();
				}

				if( mDatabaseVersion != null ) {
					databaseId = mDatabaseVersion.DatabaseId;
				}

				return( databaseId );
			}
		}

		public DbVersion DatabaseVersion {
			get { 
				if( mDatabaseVersion == null ) {
					RetrieveDatabaseVerson();
				}

				return( mDatabaseVersion );
			}
		}

		public void InitializeDatabaseVersion( Int16 databaseVersion ) {
			throw new NotImplementedException();
		}

		private void RetrieveDatabaseVerson() {
			IDatabase	database = null;

			try {
				database = mDatabaseManager.ReserveDatabase();
				mDatabaseVersion = database.DatabaseVersion;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "DatabaseInfoProvider:RetrieveDatabaseVersion", ex );
			}
			finally {
				if( database != null ) {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}
	}
}
