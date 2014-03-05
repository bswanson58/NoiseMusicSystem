using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Database {
	internal class EloqueraDb : IDatabase {
		private const Int16					cDatabaseVersion = 2;

		private readonly IIoc				mComponentCreator;
		private readonly string				mDatabaseLocation;
		private readonly string				mDatabaseName;
		private Parameters					mParameters;

		public	DB							Database { get; private set; }
		public	string						DatabaseId { get; private set; }
		public	DbVersion					DatabaseVersion { get; private set; }
		public	IBlobStorage				BlobStorage { get; set; }
		public	bool						IsOpen { get; private set; }

		[ImportMany("PersistenceType")]
		public IEnumerable<Type>	PersistenceTypes;

		public EloqueraDb( IIoc componentCreator, LibraryConfiguration databaseConfiguration ) {
			Condition.Requires( componentCreator ).IsNotNull();
			Condition.Requires( databaseConfiguration ).IsNotNull();

			mComponentCreator = componentCreator;

			DatabaseId = Guid.NewGuid().ToString();

			mDatabaseName = databaseConfiguration.DatabaseName;
			mDatabaseLocation = databaseConfiguration.DatabaseServer;
		}

		public bool InitializeDatabase() {
			var retValue = false;

			try {
				Database = new DB( string.Format( "server={0};password=;options=none;", mDatabaseLocation ));

				retValue = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( ex );
			}

			Condition.Ensures( Database ).IsNotNull();
			return( retValue );
		}

		public bool InitializeAndOpenDatabase() {
			var retValue = false;

			if( InitializeDatabase()) {
				retValue = OpenDatabase();
			}

			return( retValue );
		}

		public bool OpenWithCreateDatabase() {
			Condition.Requires( Database ).IsNotNull();

			var retValue = false;

			if(!OpenDatabase()) {
				if( CreateDatabase( mDatabaseName )) {
					if( InternalOpenDatabase()) {
						RegisterDatabaseTypes();

						var dbVersion = new DbVersion( cDatabaseVersion );
						InsertItem( dbVersion );

						CloseDatabase();

						if( OpenDatabase()) {
							retValue = true;
						}
					}
				}
			}
			else {
				retValue = true;
			}

			return( retValue );
		}

		private bool OpenDatabase() {
			Condition.Requires( Database ).IsNotNull();

			var retValue = false;

			if( InternalOpenDatabase()) {
				if( ReadDatabaseVersion()) {
					var currentVersion = new DbVersion( cDatabaseVersion );

					NoiseLogger.Current.LogMessage( String.Format( "Initialize and open '{0}' database version {1} on server '{2}'.",
														mDatabaseName, DatabaseVersion.DatabaseVersion, mDatabaseLocation ));

					if( DatabaseVersion.IsOlderVersion( currentVersion )) {
						NoiseLogger.Current.LogMessage( String.Format( "Updating database '{0}' to database version {1} on server '{2}'.",
															mDatabaseName, currentVersion.DatabaseVersion, mDatabaseLocation ));

						// Register database type should update the schema for all database types.
						RegisterDatabaseTypes();

						// Update the database version.
						DeleteItem( DatabaseVersion );
						DatabaseVersion = currentVersion;
						InsertItem( DatabaseVersion );
					}
					IsOpen = true;
					retValue = true;
				}
			}

			return( retValue );
		}

		private bool InternalOpenDatabase() {
			var			retValue = false;

			try {
				Database.OpenDatabase( mDatabaseName );
				Database.RefreshMode = ObjectRefreshMode.AlwaysReturnUpdatedValues;

				retValue = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Exception - Opening database '{0}' on server '{1}'", mDatabaseName, mDatabaseLocation ), ex );
			}

			return( retValue );
		}

		private bool ReadDatabaseVersion() {
			var retValue = false;

			DatabaseVersion = Database.ExecuteScalar( "Select DbVersion" ) as DbVersion;
			if( DatabaseVersion != null ) {
				retValue = true;
			}

			return( retValue );
		}

		public void CloseDatabase() {
			if( Database.IsOpen ) {
				Database.Close();

				IsOpen = false;
			}
		}

		public void DeleteDatabase() {
			if( IsOpen ) {
				CloseDatabase();
			}

			Database.DeleteDatabase( mDatabaseName, true );

			if( BlobStorage != null ) {
				BlobStorage.DeleteStorage();
			}
		}

		private bool CreateDatabase( string databaseName ) {
			var retValue =false;

			Condition.Requires( Database ).IsNotNull();

			try {
				Database.CreateDatabase( databaseName );
				NoiseLogger.Current.LogMessage( "Created database: '{0}' on server '{1}'.", databaseName, mDatabaseLocation );

				retValue = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Exception - Creating database '{0}' on server '{1}'", databaseName, mDatabaseLocation ), ex );
			}

			return( retValue );
		}

		private void RegisterDatabaseTypes() {
			mComponentCreator.ComposeParts( this );

			foreach( Type type in PersistenceTypes ) {
				Database.RegisterType( type );
			}
		}

		public DbBase ValidateOnThread( DbBase dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			var retValue = dbObject;

			if( Database.GetUid( dbObject ) == -1 ) {
				var parms = Database.CreateParameters();
				parms["dbid"] = dbObject.DbId;

				retValue = Database.ExecuteScalar( "SELECT DbBase WHERE DbId = @dbid", parms ) as DbBase;
			}

			Condition.Ensures( retValue ).IsNotNull();

			return( retValue );
		}

		public void InsertItem( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			if( Database.GetUid( dbObject ) == -1 ) {
				Database.Store( dbObject );
			}
			else {
				NoiseLogger.Current.LogMessage( String.Format( "Database:Insert - Inserting known item: {0}", dbObject.GetType()));
			}
		}

		public void UpdateItem( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			if( Database.GetUid( dbObject ) != -1 ) {
				Database.Store( dbObject );
			}
			else {
				NoiseLogger.Current.LogMessage( String.Format( "Database:Store - Unknown dbObject: {0}", dbObject.GetType()));
			}
		}

		public void DeleteItem( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			if( Database.GetUid( dbObject ) == -1 ) {
				if( dbObject is DbBase ) {
					dbObject = ValidateOnThread( dbObject as DbBase );
				}
				else {
					NoiseLogger.Current.LogMessage( String.Format( "Database:Delete - Unknown dbObject: {0}", dbObject.GetType()));

					return;
				}
			}

			if( dbObject != null ) {
				Database.Delete( dbObject );
			}
		}

		private Parameters QueryParameters {
			get {
				if( mParameters == null ) {
					mParameters = Database.CreateParameters();
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

			return( Database.ExecuteScalar( query ));
		}

		public object QueryForItem( string query, IDictionary<string, object> parameters ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			SetParameters( parameters );

			return( Database.ExecuteScalar( query, QueryParameters ));
		}

		public IEnumerable QueryForList( string query ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			return( Database.ExecuteQuery( query ));
		}

		public IEnumerable QueryForList( string query, IDictionary<string, object> parameters ) {
			Condition.Requires( query ).IsNotNullOrEmpty();

			SetParameters( parameters );

			return( Database.ExecuteQuery( query, QueryParameters ));
		}
	}
}
