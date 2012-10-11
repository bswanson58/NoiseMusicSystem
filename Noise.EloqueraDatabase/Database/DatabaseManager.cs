using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;

namespace Noise.EloqueraDatabase.Database {
	public class DatabaseManager : IEloqueraManager {
		private readonly object							mLockObject;
		private readonly List<IDatabase>				mAvailableDatabases;
		private readonly Dictionary<string, IDatabase>	mReservedDatabases;
		private readonly Dictionary<string, string>		mReservedStacks;
		private readonly IDatabaseFactory				mDatabaseFactory;
		private	bool									mHasShutdown;

		public DatabaseManager( IDatabaseFactory databaseFactory ) {
			if( databaseFactory == null ) {
				throw new ArgumentNullException( "databaseFactory", "Initializing DatabaseManager" );
			}

			mLockObject = new object();
			mDatabaseFactory = databaseFactory;
			mReservedDatabases = new Dictionary<string, IDatabase>();
			mReservedStacks = new Dictionary<string, string>();
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

				foreach( var stackTrace in mReservedStacks.Values ) {
					NoiseLogger.Current.LogInfo( stackTrace );
				}
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

		public IDatabaseShell CreateDatabase() {
			return( new DatabaseShell( this ));
		}

		public IDatabase ReserveDatabase() {
			IDatabase	retValue = null;

			if(!mHasShutdown ) {
				lock( mLockObject ) {
					if( mAvailableDatabases.Count > 0 ) {
						retValue = mAvailableDatabases[0];

						mAvailableDatabases.RemoveAt( 0 );
						mReservedDatabases.Add( retValue.DatabaseId, retValue );
						mReservedStacks.Add( retValue.DatabaseId, StackTraceToString());
					}
					else {
						retValue = mDatabaseFactory.GetDatabaseInstance();

						if( retValue.InitializeAndOpenDatabase()) {
							mReservedDatabases.Add( retValue.DatabaseId, retValue );
							mReservedStacks.Add( retValue.DatabaseId, StackTraceToString());

							NoiseLogger.Current.LogInfo( string.Format( "Database Created. (Count: {0})", mReservedDatabases.Count + mAvailableDatabases.Count ));
						}
					}

					mDatabaseFactory.SetBlobStorageInstance( retValue );
				}
			}

			return( retValue );
		}

		public int ReservedDatabaseCount {
			get{ return( mReservedDatabases.Count ); }
		}

		static public string StackTraceToString() {
		    var sb = new StringBuilder( 256 );
			var frames = new StackTrace().GetFrames();

			if( frames != null ) {
				for (int i = 1; i < frames.Length; i++) { /* Ignore current StackTraceToString method...*/
					var currFrame = frames[i];
					var method = currFrame.GetMethod();
				
					sb.Append(string.Format("{0}:{1} - ", method.ReflectedType != null ? method.ReflectedType.Name : string.Empty, method.Name));
				}
			}
    
			return sb.ToString();
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
						mReservedStacks.Remove( databaseId );
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
