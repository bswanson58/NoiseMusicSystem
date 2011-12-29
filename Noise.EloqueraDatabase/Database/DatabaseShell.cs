using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Database {
	internal class DatabaseShell : IDatabaseShell {
		private readonly IDatabaseManager	mDatabaseMgr;
		private IDatabase					mDatabase;

		public DatabaseShell( IDatabaseManager databaseManager ) {
			mDatabaseMgr = databaseManager;
		}

		public IDatabase Database {
			get {
				if( mDatabase == null ) {
					mDatabase = mDatabaseMgr.ReserveDatabase();

//					NoiseLogger.Current.LogInfo( string.Format( "Reserving database: {0}", mDatabase.DatabaseId ));
				}

				return( mDatabase );
			}
		}

		public void FreeDatabase() {
			Dispose();
		}

		public void Dispose() {
			if( mDatabase != null ) {
				mDatabaseMgr.FreeDatabase( mDatabase );

//				NoiseLogger.Current.LogInfo( string.Format( "Freeing database: {0}", mDatabase.DatabaseId ));

				mDatabase = null;
			}
		}
	}
}
