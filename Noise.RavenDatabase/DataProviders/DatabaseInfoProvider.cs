using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class DatabaseInfoProvider : BaseProvider<DbVersion>, IDatabaseInfo {
		private DbVersion		mDatabaseVersion;

		public DatabaseInfoProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
		}

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

		public bool IsOpen {
			get { return( DbFactory.IsOpen ); }
		}

		public void InitializeDatabaseVersion( short databaseVersion ) {
			RetrieveDatabaseVersion();

			if( mDatabaseVersion != null ) {
				Database.Delete( mDatabaseVersion );

				mDatabaseVersion = null;
			}

			mDatabaseVersion = new DbVersion( databaseVersion );
			Database.Add( mDatabaseVersion );
		}

		private void RetrieveDatabaseVersion() {
			mDatabaseVersion = Database.Get( DbVersion.DatabaseVersionDbId );
		}
	}
}
