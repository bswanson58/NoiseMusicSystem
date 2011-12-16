using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Database {
	internal class EloqueraDb : IDatabase {
		private const UInt16				cDatabaseVersionMajor = 0;
		private const UInt16				cDatabaseVersionMinor = 5;

		private readonly IEventAggregator	mEventAggregator;
		private readonly IIoc				mComponentCreator;
		private readonly string				mDatabaseLocation;
		private readonly string				mDatabaseName;

		public	DB							Database { get; private set; }
		public	string						DatabaseId { get; private set; }
		public	DbVersion					DatabaseVersion { get; private set; }
		public	IBlobStorage				BlobStorage { get; set; }
		public	bool						IsOpen { get; private set; }

		[ImportMany("PersistenceType")]
		public IEnumerable<Type>	PersistenceTypes;

		public EloqueraDb( IEventAggregator eventAggregator, IIoc componentCreator ) {
			mEventAggregator = eventAggregator;
			mComponentCreator = componentCreator;

			DatabaseId = Guid.NewGuid().ToString();

			var config = NoiseSystemConfiguration.Current.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

			if( config != null ) {
				mDatabaseName = config.DatabaseName;
				mDatabaseLocation = config.ServerName;
			}
			else {
				NoiseLogger.Current.LogMessage( "Database configuration could not be loaded." );
			}
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

						var dbVersion = new DbVersion( cDatabaseVersionMajor, cDatabaseVersionMinor );
						Insert( dbVersion );

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
					NoiseLogger.Current.LogMessage( String.Format( "Initialize and open '{0}' database version {1}.{2} on server '{3}'.",
														mDatabaseName, DatabaseVersion.MajorVersion, DatabaseVersion.MinorVersion, mDatabaseLocation ));
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

		public void Insert( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			if( Database.GetUid( dbObject ) == -1 ) {
				Database.Store( dbObject );

				if( dbObject is DbBase ) {
					mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( dbObject as DbBase, DbItemChanged.Insert ) );
				}
			}
			else {
				NoiseLogger.Current.LogMessage( String.Format( "Database:Insert - Inserting known item: {0}", dbObject.GetType()));
			}
		}

		public void Store( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			if( Database.GetUid( dbObject ) != -1 ) {
				Database.Store( dbObject );

				if( dbObject is DbBase ) {
					mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( dbObject as DbBase, DbItemChanged.Update ));
				}
			}
			else {
				NoiseLogger.Current.LogMessage( String.Format( "Database:Store - Unknown dbObject: {0}", dbObject.GetType()));
			}
		}

		public void Delete( object dbObject ) {
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

				if( dbObject is DbBase ) {
					mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( dbObject as DbBase, DbItemChanged.Delete ));
				}
			}
		}
	}
}
