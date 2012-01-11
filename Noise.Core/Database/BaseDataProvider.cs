using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal abstract class BaseDataProvider<T> where T : class {
		private readonly IDatabaseManager	mDatabaseMgr;

		protected  BaseDataProvider( IDatabaseManager databaseManager ) {
			mDatabaseMgr = databaseManager;
		}

		protected IDatabaseShell CreateDatabase() {
			return ( mDatabaseMgr.CreateDatabase());
		}

		protected long GetItemCount( string query ) {
			var retValue = 0L;

			try {
				using( var itemList = TryGetList( query, "GetItemCount" )) {
					retValue = itemList.List.Count();
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "GetItemCount", ex );
			}

			return( retValue );
		}

		protected T GetItem( string query ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			T retValue;

			using( var dbShell = CreateDatabase()) {
				retValue = dbShell.Database.QueryForItem( query ) as T;
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

			T retValue;

			using( var dbShell = CreateDatabase()) {
				retValue = GetItem( query, parms, dbShell );
			}

			return( retValue );
		}

		protected T GetItem( string query, IDictionary<string, object> parms, IDatabaseShell database ) {
			Condition.Requires( query ).IsNotNullOrEmpty();
			Condition.Requires( parms ).IsNotEmpty();
			Condition.Requires( database ).IsNotNull();

			return( database.Database.QueryForItem( query, parms ) as T );
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

			var dbShell = CreateDatabase();

			return( new DataProviderList<T>( dbShell, dbShell.Database.QueryForList( query ).OfType<T>()));
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

		protected DataProviderList<T> GetList( string query, IDictionary<string, object> parameters ) {
			Condition.Requires( query ).IsNotNullOrEmpty();
			Condition.Requires( parameters ).IsNotEmpty();

			var dbShell = CreateDatabase();

			return( new DataProviderList<T>( dbShell, dbShell.Database.QueryForList( query, parameters ).OfType<T>()));
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

		protected DataUpdateShell<T> GetUpdateShell( string query, IDictionary<string, object> parameters ) {
			Condition.Requires( query ).IsNotNullOrEmpty();
			Condition.Requires( parameters ).IsNotEmpty();

			var dbShell = CreateDatabase();

			return( new DataUpdateShell<T>( dbShell, dbShell.Database.QueryForItem( query, parameters ) as T ));
		}

		protected void InsertItem( T item ) {
			using( var dbShell = CreateDatabase()) {
				dbShell.Database.InsertItem( item );
			}
		}

		protected void DeleteItem( T item ) {
			using( var dbShell = CreateDatabase()) {
				dbShell.Database.DeleteItem( item );
			}
		}
	}
}
