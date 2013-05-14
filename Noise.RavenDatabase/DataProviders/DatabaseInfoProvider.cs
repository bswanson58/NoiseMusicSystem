using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class DatabaseInfoProvider : BaseProvider<DbVersion>, IDatabaseInfo {
		private DbVersion		mDatabaseVersion;

		public DatabaseInfoProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
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
			get { return( false ); }
		}

		public void InitializeDatabaseVersion( short majorVersion, short minorVersion ) {
			RetrieveDatabaseVersion();

			if( mDatabaseVersion != null ) {
				Database.Delete( mDatabaseVersion );

				mDatabaseVersion = null;
			}

			mDatabaseVersion = new DbVersion( majorVersion, minorVersion );
			Database.Add( mDatabaseVersion );
		}

		private void RetrieveDatabaseVersion() {
			mDatabaseVersion = Database.Get( DbVersion.DatabaseVersionDbId );
		}
	}
}
