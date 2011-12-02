using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Practices.Unity;
using Noise.Core.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	class DatabaseManager : IDatabaseManager {
		private const string	cBlobStorageName	= "Noise Blobs";

		private readonly object					mLockObject;
		private readonly IUnityContainer		mContainer;
		private readonly ILog					mLog;
		private readonly List<IDatabase>		mAvailableDatabases;
		private readonly Dictionary<string, IDatabase>	mReservedDatabases;
		private readonly IBlobStorageManager	mBlobStorageMgr;
		private	bool							mHasShutdown;

		public DatabaseManager( IUnityContainer container ) {
			mLockObject = new object();
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
			mReservedDatabases = new Dictionary<string, IDatabase>();
			mAvailableDatabases = new List<IDatabase>();

			var blobStoragePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );
			mBlobStorageMgr = new BlobStorageManager( new BlobStorageResolver(), blobStoragePath );
		}

		public bool Initialize() {
			var retValue = false;

			var database = mContainer.Resolve<IDatabase>();
			if( database.InitializeDatabase()) {
				if( database.OpenWithCreateDatabase()) {
					mAvailableDatabases.Add( database );

					var blobStorage = Path.Combine( cBlobStorageName, database.DatabaseVersion.DatabaseId.ToString());
					retValue = mBlobStorageMgr.OpenStorage( blobStorage );
					if(!retValue ) {
						mBlobStorageMgr.CreateStorage( blobStorage );

						retValue = mBlobStorageMgr.OpenStorage( blobStorage );
					}
				}
			}

			return( retValue );
		}

		public void Shutdown() {
			if( mReservedDatabases.Count > 0 ) {
				mLog.LogMessage( string.Format( "DatabaseManager has {0} reserved databases on shutdown!", mReservedDatabases.Count ));
			}
			mLog.LogMessage( string.Format( "DatabaseManager closing {0} databases.", mReservedDatabases.Count + mAvailableDatabases.Count ));

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

			mBlobStorageMgr.CloseStorage();
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
						retValue = mContainer.Resolve<IDatabase>();

						if( retValue.InitializeAndOpenDatabase()) {
							mReservedDatabases.Add( retValue.DatabaseId, retValue );

							mLog.LogInfo( string.Format( "Database Created. (Count: {0})", mReservedDatabases.Count + mAvailableDatabases.Count ));
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
						mLog.LogMessage( string.Format( "Database ID not reserved:{0}", databaseId ));
					}
				}
			}
		}

		public IBlobStorage GetBlobDatabase() {
			return( mBlobStorageMgr.GetStorage());
		}
	}
}
