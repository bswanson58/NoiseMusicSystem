using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	class DatabaseManager : IDatabaseManager {
		private readonly object				mLockObject;
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private readonly List<IDatabase>	mAvailableDatabases;
		private readonly Dictionary<string, IDatabase>	mReservedDatabases;

		public DatabaseManager( IUnityContainer container ) {
			mLockObject = new object();
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
			mReservedDatabases = new Dictionary<string, IDatabase>();
			mAvailableDatabases = new List<IDatabase>();
		}

		public bool Initialize() {
			var retValue = false;

			var database = mContainer.Resolve<IDatabase>();
			if( database.InitializeDatabase()) {
				if( database.OpenWithCreateDatabase()) {
					mAvailableDatabases.Add( database );

					retValue = true;
				}
			}

			return( retValue );
		}

		public void Shutdown() {
			lock( mLockObject ) {
				foreach( var database in mReservedDatabases.Values ) {
					database.CloseDatabase();
				}

				mReservedDatabases.Clear();

				foreach( var database in mAvailableDatabases ) {
					database.CloseDatabase();
				}

				mAvailableDatabases.Clear();
			}
		}

		public IDatabase ReserveDatabase( string clientName ) {
			IDatabase	retValue;

			lock( mLockObject ) {
				if( mAvailableDatabases.Count > 0 ) {
					retValue = mAvailableDatabases[0];

					mAvailableDatabases.RemoveAt( 0 );
					mReservedDatabases.Add( retValue.DatabaseId, retValue );
				}
				else {
					retValue = mContainer.Resolve<IDatabase>();

					if( retValue.InitializeAndOpenDatabase( clientName )) {
						mReservedDatabases.Add( retValue.DatabaseId, retValue );

						mLog.LogInfo( string.Format( "Database Created. (Count: {0})", mReservedDatabases.Count + mAvailableDatabases.Count ));
					}
				}
			}

			return( retValue );
		}

		public void FreeDatabase( string databaseId ) {
			lock( mLockObject ) {
				if( mReservedDatabases.ContainsKey( databaseId )) {
					var database = mReservedDatabases[databaseId];

					mReservedDatabases.Remove( databaseId );
					mAvailableDatabases.Add( database );
				}
				else {
					mLog.LogMessage( string.Format( "Database ID not reserved:{0}", databaseId ));
				}
			}
		}
	}
}
