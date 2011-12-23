using System.Collections;
using System.Collections.Generic;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class DatabaseShell : IDatabaseShell {
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

		private Parameters QueryParameters {
			get {
				if( mParameters == null ) {
					mParameters = Database.Database.CreateParameters();
				}

				return( mParameters );
			}
		}

		private void SetParameter( string parameter, object value ) {
			Condition.Requires( parameter ).IsNotNullOrEmpty();
			Condition.Requires( value ).IsNotNull();

			QueryParameters[parameter] = value;
		}

		private void SetParameters( IDictionary<string, object> parameters ) {
			Condition.Requires( parameters ).IsNotNull();
			Condition.Requires( parameters ).IsNotEmpty();

			foreach( var value in parameters ) {
				SetParameter( value.Key, value.Value );
			}
		}

		public object QueryForItem( string query ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			return( Database.Database.ExecuteScalar( query ));
		}

		public object QueryForItem( string query, IDictionary<string, object> parameters ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			SetParameters( parameters );

			return( Database.Database.ExecuteScalar( query, QueryParameters ));
		}

		public IEnumerable QueryForList( string query ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			return( Database.Database.ExecuteQuery( query ));
		}

		public IEnumerable QueryForList( string query, IDictionary<string, object> parameters ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			SetParameters( parameters );

			return( Database.Database.ExecuteQuery( query, QueryParameters ));
		}

		public void InsertItem( object item ) {
			Database.Insert( item );
		}

		public void UpdateItem( object item ) {
			Database.Store( item );
		}

		public void DeleteItem( object item ) {
			Database.Delete( item );
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
