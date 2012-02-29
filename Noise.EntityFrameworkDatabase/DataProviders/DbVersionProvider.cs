using System;
using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class DbVersionProvider : BaseProvider<DbVersion>, IDatabaseInfo {
		private DbVersion		mDatabaseVersion;

		public DbVersionProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public long DatabaseId {
			get {
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

		public void InitializeDatabaseVersion( Int16 majorVersion, Int16 minorVersion ) {
			var dbVersion = new DbVersion( majorVersion, minorVersion );

			using( var context = CreateContext()) {
				Set( context ).Add( dbVersion );

				context.SaveChanges();
			}
		}

		private void RetrieveDatabaseVersion() {
			using( var context = CreateContext()) {
				mDatabaseVersion = Set( context ).FirstOrDefault( entity => entity.DbId == DbVersion.DatabaseVersionDbId );
			}
		}
	}
}
