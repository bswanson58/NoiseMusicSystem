using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class EloqueraDatabase : IDatabase {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private readonly string				mDatabaseLocation;
		private readonly string				mDatabaseName;

		public	DB		Database { get; private set; }
		public	string	DatabaseId { get; private set; }

		[ImportMany("PersistenceType")]
		public IEnumerable<Type>	PersistenceTypes;

		public EloqueraDatabase( IUnityContainer container ) {
			mContainer = container;
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

		public bool InitializeAndOpenDatabase( string clientName) {
			Condition.Requires( clientName ).IsNotNullOrEmpty();

			var retValue = false;

			mLog.LogMessage( String.Format( "{0} - Initialize and open '{1}' database on server '{2}'.", clientName, mDatabaseName, mDatabaseLocation ));
			if( InitializeDatabase()) {
				retValue = InternalOpenDatabase();
			}

			return( retValue );
		}

		public bool OpenWithCreateDatabase() {
			var retValue = false;

			if(!OpenDatabase()) {
				if( CreateDatabase( mDatabaseName )) {
					retValue = InternalOpenDatabase();

					RegisterDatabaseTypes();
					CloseDatabase();
					OpenDatabase();
				}
			}
			else {
				retValue = true;
			}

			return( retValue );
		}

		public bool OpenDatabase() {
			Condition.Requires( Database ).IsNotNull();

			mLog.LogMessage( "Opening database '{0}' on server '{1}'", mDatabaseName, mDatabaseLocation );

			return ( InternalOpenDatabase());
		}

		private bool InternalOpenDatabase() {
			var			retValue = false;

			try {
				Database.OpenDatabase( mDatabaseName );

				retValue = true;
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Opening database '{0}' on server '{1}' failed", mDatabaseName, mDatabaseLocation ), ex );
			}

			return( retValue );
		}

		public void CloseDatabase( string clientName ) {
			Condition.Requires( clientName ).IsNotNullOrEmpty();

			mLog.LogMessage( String.Format( "{0} - Closing database '{1}' on server '{2}'.", clientName, mDatabaseName, mDatabaseLocation ));

			CloseDatabase();
		}

		public void CloseDatabase() {
			if( Database.IsOpen ) {
				Database.Close();
			}
		}

		private bool CreateDatabase( string databaseName ) {
			var retValue =false;

			Condition.Requires( Database ).IsNotNull();
			mLog.LogMessage( "Creating database: '{0}' on server '{1}'.", databaseName, mDatabaseLocation );

			try {
				Database.CreateDatabase( databaseName );

				retValue = true;
			}
			catch( Exception ex ) {
				mLog.LogException( ex );
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
	}
}
