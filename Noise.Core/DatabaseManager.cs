using System;
using System.ComponentModel.Composition;
using Eloquera.Client;
using Noise.Infrastructure;

namespace Noise.Core {
	[Export(typeof(IDatabaseManager))]
	public class DatabaseManager : IDatabaseManager {
		private DB		mDatabase;

		[Import]
		private ILog		mLog;

		public bool InitializeDatabase( string databaseLocation ) {
			var retValue = true;

			try {
				mDatabase = new DB( string.Format( "server={0};password=;options=none;", databaseLocation ) );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );

				retValue = false;
			}

			return( retValue );
		}

		public void OpenWithCreateDatabase( string databaseName ) {
			if(!OpenDatabase( databaseName )) {
				CreateDatabase( databaseName );
				OpenDatabase( databaseName );
			}
		}

		public bool OpenDatabase( string databaseName ) {
			var retValue = true;

			try {
				mDatabase.OpenDatabase( databaseName );
			}
			catch( Exception ex ) {
				mLog.LogException( "Opening database failed", ex );

				retValue = false;
			}

			return ( retValue );
		}

		private void CreateDatabase( string databaseName ) {
			mLog.LogMessage( "Creating Noise database." );

			try {
				mDatabase.CreateDatabase( databaseName );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );
			}
		}
	}
}
