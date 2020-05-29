using System;
using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class DbVersionProvider : BaseProvider<DbVersion>, IDatabaseInfo {
		private DbVersion		mDatabaseVersion;
		private bool			mIsOpen;

		public DbVersionProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

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
				var retValue = mIsOpen;

				using( var context = CreateContext()) {
					retValue &= context.IsValidContext;
				}

				return( retValue );
			}
		}

        public void SetDatabaseClosed() {
            mIsOpen = false;
        }

		public void InitializeDatabaseVersion( Int16 databaseVersion ) {
			var dbVersion = new DbVersion( databaseVersion );

			using( var context = CreateContext()) {
				Set( context ).Add( dbVersion );

				context.SaveChanges();

				mIsOpen = true;
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
