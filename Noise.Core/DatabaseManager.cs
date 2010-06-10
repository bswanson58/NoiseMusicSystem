using System;
using System.ComponentModel.Composition;
using Eloquera.Client;
using Noise.Infrastructure;

namespace Noise.Core {
	[Export(typeof(IDatabaseManager))]
	public class DatabaseManager : IDatabaseManager {
		private string	mDatabaseLocation;
		private string	mDatabaseName;
		private DB		mDatabase;

		[Import]
		private ILog		mLog;

		public bool InitializeDatabase( string databaseLocation ) {
			var retValue = true;

			mDatabaseLocation = databaseLocation;

			try {
				mDatabase = new DB( string.Format( "server={0};password=;options=none;", mDatabaseLocation ) );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );

				retValue = false;
			}

			return( retValue );
		}

		public void OpenWithCreateDatabase( string databaseName ) {
			mDatabaseName = databaseName;

			if(!OpenDatabase( mDatabaseName )) {
				CreateDatabase( mDatabaseName );
				OpenDatabase( mDatabaseName );
			}
		}

		public bool OpenDatabase( string databaseName ) {
			var retValue = true;

			mDatabaseName = databaseName;

			try {
				mDatabase.OpenDatabase( mDatabaseName );
				mLog.LogMessage( "Opened database: {0} on server: {1}", mDatabaseName, mDatabaseLocation );
			}
			catch( Exception ex ) {
				mLog.LogException( "Opening database failed", ex );

				retValue = false;
			}

			return ( retValue );
		}

		private void CreateDatabase( string databaseName ) {
			mLog.LogMessage( "Creating Noise database: {0}", databaseName );

			try {
				mDatabase.CreateDatabase( databaseName );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );
			}
		}
	}
}
