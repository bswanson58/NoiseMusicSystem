using System;
using System.Collections.Generic;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DatabaseManager : IDatabaseManager {
		private readonly object					mLockObject;
		private readonly List<IDatabase>		mAvailableDatabases;
		private readonly Dictionary<string, IDatabase>	mReservedDatabases;
		private readonly IDatabaseFactory		mDatabaseFactory;
		private	bool							mHasShutdown;

		public DatabaseManager( IDatabaseFactory databaseFactory ) {
			if( databaseFactory == null ) {
				throw new ArgumentNullException( "databaseFactory", "Initializing DatabaseManager" );
			}

			mLockObject = new object();
			mDatabaseFactory = databaseFactory;
			mReservedDatabases = new Dictionary<string, IDatabase>();
			mAvailableDatabases = new List<IDatabase>();

			NoiseLogger.Current.LogInfo( "DatabaseManager created" );
		}

		public bool Initialize() {
			var retValue = false;

			var database = mDatabaseFactory.GetDatabaseInstance();
			if( database.InitializeDatabase()) {
				if( database.OpenWithCreateDatabase()) {
					mDatabaseFactory.SetBlobStorageInstance( database );
					mAvailableDatabases.Add( database );

					retValue = true;
				}
			}

			return( retValue );
		}

		public void Shutdown() {
			if( mReservedDatabases.Count > 0 ) {
				NoiseLogger.Current.LogMessage( string.Format( "DatabaseManager has {0} reserved databases on shutdown!", mReservedDatabases.Count ));
			}
			NoiseLogger.Current.LogMessage( string.Format( "DatabaseManager closing {0} databases.", mReservedDatabases.Count + mAvailableDatabases.Count ));

			lock( mLockObject ) {
				foreach( var database in mReservedDatabases.Values ) {
					database.CloseDatabase();
				}

				mReservedDatabases.Clear();

				foreach( var database in mAvailableDatabases ) {
					database.CloseDatabase();
				}

				mAvailableDatabases.Clear();
				mHasShutdown = true;
			}

			mDatabaseFactory.CloseFactory();
		}

		public IDatabase ReserveDatabase() {
			IDatabase	retValue = null;

			if(!mHasShutdown ) {
				lock( mLockObject ) {
					if( mAvailableDatabases.Count > 0 ) {
						retValue = mAvailableDatabases[0];

						mAvailableDatabases.RemoveAt( 0 );
						mReservedDatabases.Add( retValue.DatabaseId, retValue );
					}
					else {
						retValue = mDatabaseFactory.GetDatabaseInstance();

						if( retValue.InitializeAndOpenDatabase()) {
							mDatabaseFactory.SetBlobStorageInstance( retValue );
							mReservedDatabases.Add( retValue.DatabaseId, retValue );

							NoiseLogger.Current.LogInfo( string.Format( "Database Created. (Count: {0})", mReservedDatabases.Count + mAvailableDatabases.Count ));
						}
					}
				}
			}

			return( retValue );
		}

		public IDatabase GetDatabase( string databaseId ) {
			IDatabase	retValue = null;

			if( mReservedDatabases.ContainsKey( databaseId )) {
				retValue = mReservedDatabases[databaseId];
			}

			return( retValue );
		}

		public void FreeDatabase( IDatabase database ) {
			if( database != null ) {
				FreeDatabase( database.DatabaseId );
			}
		}

		public void FreeDatabase( string databaseId ) {
			lock( mLockObject ) {
				if(!mHasShutdown ) {
					if( mReservedDatabases.ContainsKey( databaseId )) {
						var database = mReservedDatabases[databaseId];

						mReservedDatabases.Remove( databaseId );
						mAvailableDatabases.Add( database );
					}
					else {
						NoiseLogger.Current.LogMessage( string.Format( "Database ID not reserved:{0}", databaseId ));
					}
				}
			}
		}
	}
}
