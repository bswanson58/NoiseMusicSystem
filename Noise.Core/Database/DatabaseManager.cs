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
	public class DatabaseManager : IDatabaseManager, IDisposable {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private readonly string				mDatabaseLocation;
		private readonly string				mDatabaseName;

		public	DB		Database { get; private set; }

		[ImportMany("PersistenceType")]
		public IEnumerable<Type>	PersistenceTypes;

		public DatabaseManager( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();

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

		public void OpenWithCreateDatabase() {
			if(!OpenDatabase()) {
				CreateDatabase( mDatabaseName );
				OpenDatabase();
			}
		}

		public bool OpenDatabase() {
			Condition.Requires( Database ).IsNotNull();

			var retValue = true;

			try {
				Database.OpenDatabase( mDatabaseName );
				RegisterDatabaseTypes();

				mLog.LogMessage( "Opened database: {0} on server: {1}", mDatabaseName, mDatabaseLocation );
			}
			catch( Exception ex ) {
				mLog.LogException( "Opening database failed", ex );

				retValue = false;
			}

			return ( retValue );
		}

		private void CreateDatabase( string databaseName ) {
			Condition.Requires( Database ).IsNotNull();
			mLog.LogMessage( "Creating Noise database: {0}", databaseName );

			try {
				Database.CreateDatabase( databaseName );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );
			}
		}

		private void RegisterDatabaseTypes() {
			var catalog = new DirectoryCatalog(  @".\" );
			var container = new CompositionContainer( catalog );
			container.ComposeParts( this );

			foreach( Type type in PersistenceTypes ) {
				Database.RegisterType( type );
			}
		}

		public void Dispose() {
			if( Database != null ) {
				Database.Close();

				Database = null;
			}
		}
	}
}
