using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class DatabaseInfoProvider : IDatabaseInfo {
		private readonly IDbFactory				mDbFactory;
		private readonly IRepository<DbVersion>	mDatabase;
		private DbVersion						mDatabaseVersion;

		public long DatabaseId {
			get{
				var retValue = 0L;

				if( mDatabaseVersion == null ) {
					RetrieveDatabaseVersion();
				}

				if( mDatabaseVersion != null ) {
					retValue = mDatabaseVersion.DatabaseId;
				}

				return( retValue );
			}
		}

		public DbVersion DatabaseVersion {
			get {
				if( mDatabaseVersion == null ) {
					RetrieveDatabaseVersion();
				}

				return( mDatabaseVersion );
			}
		}

		public DatabaseInfoProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbVersion>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public bool IsOpen {
			get { return( false ); }
		}

		public void InitializeDatabaseVersion( short majorVersion, short minorVersion ) {
			RetrieveDatabaseVersion();

			if( mDatabaseVersion != null ) {
				mDatabase.Delete( mDatabaseVersion );

				mDatabaseVersion = null;
			}

			mDatabaseVersion = new DbVersion( majorVersion, minorVersion );
			mDatabase.Add( mDatabaseVersion );
		}

		private void RetrieveDatabaseVersion() {
			mDatabaseVersion = mDatabase.Get( DbVersion.DatabaseVersionDbId );
		}
	}
}
