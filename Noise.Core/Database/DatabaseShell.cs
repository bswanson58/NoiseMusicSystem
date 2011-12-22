using System;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class DatabaseShell : IDisposable, IDatabaseShell {
		private readonly IDatabaseManager	mDatabaseMgr;
		private IDatabase					mDatabase;
		private Parameters					mParameters;

		public DatabaseShell( IDatabaseManager databaseManager ) {
			mDatabaseMgr = databaseManager;
		}

		public IDatabase Database {
			get {
				if( mDatabase == null ) {
					mDatabase = mDatabaseMgr.ReserveDatabase();

//					NoiseLogger.Current.LogInfo( string.Format( "Reserving database: {0}", mDatabase.DatabaseId ));
				}

				return( mDatabase );
			}
		}

		public void SetParameter( string parameter, object value ) {
			Condition.Requires( parameter ).IsNotNullOrEmpty();
			Condition.Requires( value ).IsNotNull();

			QueryParameters[parameter] = value;
		}

		public Parameters QueryParameters {
			get {
				if( mParameters == null ) {
					mParameters = Database.Database.CreateParameters();
				}

				return( mParameters );
			}
		}

		public void FreeDatabase() {
			Dispose();
		}

		public void Dispose() {
			if( mDatabase != null ) {
				mDatabaseMgr.FreeDatabase( mDatabase );

//				NoiseLogger.Current.LogInfo( string.Format( "Freeing database: {0}", mDatabase.DatabaseId ));

				mDatabase = null;
			}
		}
	}
}
