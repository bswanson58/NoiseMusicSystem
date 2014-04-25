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

		public bool IsOpen {
			get {
				bool retValue;

				using( var context = CreateContext()) {
					retValue = context.IsValidContext;
				}

				return( retValue );
			}
		}

		public void InitializeDatabaseVersion( Int16 databaseVersion ) {
			var dbVersion = new DbVersion( databaseVersion );

			using( var context = CreateContext()) {
				Set( context ).Add( dbVersion );

				context.SaveChanges();
			}
		}

		private void RetrieveDatabaseVersion() {
			using( var context = CreateContext()) {
				if( context.IsValidContext ) {
					mDatabaseVersion = Set( context ).FirstOrDefault( entity => entity.DbId == DbVersion.DatabaseVersionDbId );
				}
			}
		}
	}
}
