using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class EloqueraDatabase : IDatabase {
		private const UInt16				cDatabaseVersionMajor = 0;
		private const UInt16				cDatabaseVersionMinor = 2;

		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEventAggregator;
		private readonly ILog				mLog;
		private readonly string				mDatabaseLocation;
		private readonly string				mDatabaseName;

		public	DB			Database { get; private set; }
		public	string		DatabaseId { get; private set; }
		public	DbVersion	DatabaseVersion { get; private set; }

		[ImportMany("PersistenceType")]
		public IEnumerable<Type>	PersistenceTypes;

		public EloqueraDatabase( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();
			DatabaseId = Guid.NewGuid().ToString();

			var configMgr = mContainer.Resolve<ISystemConfiguration>();
			var config = configMgr.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

			if( config != null ) {
				mDatabaseName = config.DatabaseName;
				mDatabaseLocation = config.ServerName;
			}
			else {
				mLog.LogMessage( "Database configuration could not be loaded." );
			}
		}

		public bool InitializeDatabase() {
			var retValue = false;

			try {
				Database = new DB( string.Format( "server={0};password=;options=none;", mDatabaseLocation ));

				retValue = true;
			}
			catch( Exception ex ) {
				mLog.LogException( ex );
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

		public bool OpenDatabase() {
			Condition.Requires( Database ).IsNotNull();

			var retValue = false;

			if( InternalOpenDatabase()) {
				if( ReadDatabaseVersion()) {
					mLog.LogMessage( String.Format( "Initialize and open '{0}' database version {1}.{2} on server '{3}'.",
														mDatabaseName, DatabaseVersion.MajorVersion, DatabaseVersion.MinorVersion, mDatabaseLocation ));

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
				mLog.LogException( String.Format( "Exception - Opening database '{0}' on server '{1}'", mDatabaseName, mDatabaseLocation ), ex );
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
			}
		}

		private bool CreateDatabase( string databaseName ) {
			var retValue =false;

			Condition.Requires( Database ).IsNotNull();

			try {
				Database.CreateDatabase( databaseName );
				mLog.LogMessage( "Created database: '{0}' on server '{1}'.", databaseName, mDatabaseLocation );

				retValue = true;
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Exception - Creating database '{0}' on server '{1}'", databaseName, mDatabaseLocation ), ex );
			}

			return( retValue );
		}

		private void RegisterDatabaseTypes() {
			var catalog = new DirectoryCatalog(  @".\" );
			var container = new CompositionContainer( catalog );
			container.ComposeParts( this );

			foreach( Type type in PersistenceTypes ) {
				Database.RegisterType( type );
			}
		}

		public object ValidateOnThread( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();
			Condition.Requires( dbObject ).IsOfType( typeof( DbBase ));

			var retValue = dbObject;

			if(( Database.GetUid( dbObject ) == -1 ) &&
			   ( dbObject is DbBase )) {
				var dbId = ( dbObject as DbBase ).DbId;

				retValue = ( from DbBase o in Database where o.DbId == dbId select o ).FirstOrDefault();
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
				mLog.LogMessage( String.Format( "Database:Insert - Inserting known item: {0}", dbObject.GetType()));
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
				mLog.LogMessage( String.Format( "Database:Store - Unknown dbObject: {0}", dbObject.GetType()));
			}
		}

		public void Delete( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			if( Database.GetUid( dbObject ) != -1 ) {
				Database.Delete( dbObject );

				if( dbObject is DbBase ) {
					mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( dbObject as DbBase, DbItemChanged.Delete ));
				}
			}
			else {
				mLog.LogMessage( String.Format( "Database:Delete - Unknown dbObject: {0}", dbObject.GetType()));
			}
		}
	}
}
