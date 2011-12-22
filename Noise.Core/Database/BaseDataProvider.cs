using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DatabaseShell : IDisposable, IDatabaseShell {
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

				mDatabase = null;
			}
		}
	}

	public abstract class BaseDataProvider<T> where T : class {
		private readonly IDatabaseManager	mDatabaseMgr;

		protected  BaseDataProvider( IDatabaseManager databaseManager ) {
			mDatabaseMgr = databaseManager;
		}

		protected DatabaseShell	GetDatabase {
			get{ return( new DatabaseShell( mDatabaseMgr )); }
		}

		protected T GetItem( string query ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			T retValue;

			using( var dbShell = GetDatabase ) {
				retValue = dbShell.Database.Database.ExecuteScalar( query ) as T;
			}

			return( retValue );
		}

		protected T TryGetItem( string query, string exceptionMessage ) {
			Condition.Requires( exceptionMessage ).IsNotNullOrEmpty();

			T	retValue = null;
			try {
				retValue = GetItem( query );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( exceptionMessage, ex );
			}

			return( retValue );
		}

		protected T GetItem( string query, IDictionary<string, object> parms ) {
			Condition.Requires( query ).IsNotNullOrEmpty();
			Condition.Requires( parms ).IsNotEmpty();

			T retValue = null;

			using( var dbShell = GetDatabase ) {
				if(( parms != null ) &&
				   ( parms.Count > 0 )) {
					foreach( var entry in parms ) {
						dbShell.SetParameter( entry.Key, entry.Value );
					}

					retValue = dbShell.Database.Database.ExecuteScalar( query, dbShell.QueryParameters ) as T;
				}
			}

			return( retValue );
		}

		protected T TryGetItem( string query, IDictionary<string, object> parms, string exceptionMessage ) {
			Condition.Requires( exceptionMessage ).IsNotNullOrEmpty();

			T	retValue = null;

			try {
				retValue = GetItem( query, parms );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( exceptionMessage, ex );
			}

			return( retValue );
		}

		protected DataProviderList<T> GetList( string query ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			var dbShell = GetDatabase;

			return( new DataProviderList<T>( dbShell, dbShell.Database.Database.ExecuteQuery( query ).OfType<T>()));
		}

		protected DataProviderList<T> TryGetList( string query, string exceptionMessage ) {
			Condition.Requires( exceptionMessage ).IsNotNullOrEmpty();

			DataProviderList<T>	retValue = null;

			try {
				retValue = GetList( query );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( exceptionMessage, ex );
			}

			return( retValue );
		}

		protected DataProviderList<T> GetList( string query, IDictionary<string, object> parms ) {
			Condition.Requires( query ).IsNotNullOrEmpty();
			Condition.Requires( parms ).IsNotEmpty();

			DataProviderList<T>	retValue = null;
			var dbShell = GetDatabase;

			if(( parms != null ) &&
			   ( parms.Count > 0 )) {
				foreach( var entry in parms ) {
					dbShell.SetParameter( entry.Key, entry.Value );
				}

				retValue = new DataProviderList<T>( dbShell, dbShell.Database.Database.ExecuteQuery( query, dbShell.QueryParameters ).OfType<T>());
			}

			return( retValue );
		}

		protected DataProviderList<T> TryGetList( string query, IDictionary<string, object> parms, string exceptionMessage ) {
			Condition.Requires( exceptionMessage ).IsNotNullOrEmpty();

			DataProviderList<T>	retValue = null;

			try {
				retValue = GetList( query, parms );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( exceptionMessage, ex );
			}

			return( retValue );
		}

		protected DataUpdateShell<T> GetUpdateShell( string query, IDictionary<string, object> parms ) {
			Condition.Requires( query ).IsNotNullOrEmpty();
			Condition.Requires( parms ).IsNotEmpty();

			DataUpdateShell<T> retValue = null;

			var dbShell = GetDatabase;
			if(( parms != null ) &&
			   ( parms.Count > 0 )) {
				foreach( var entry in parms ) {
					dbShell.SetParameter( entry.Key, entry.Value );
				}

				retValue = new DataUpdateShell<T>( dbShell, Update,
													dbShell.Database.Database.ExecuteScalar( query, dbShell.QueryParameters ) as T );
			}

			return( retValue );
		}

		protected void InsertItem( T item ) {
			using( var dbShell = GetDatabase ) {
				dbShell.Database.Database.Store( item );
			}
		}

		protected void DeleteItem( T item ) {
			using( var dbShell = GetDatabase ) {
				dbShell.Database.Database.Delete( item );
			}
		}

		private void Update( string databaseId, T item ) {
			IDatabase	database = mDatabaseMgr.GetDatabase( databaseId );

			if( database != null ) {
				database.Store( item );
			}
		}
	}
}
