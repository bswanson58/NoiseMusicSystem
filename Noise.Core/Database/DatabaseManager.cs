using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DatabaseManager : IDatabaseManager {
		private readonly ILog				mLog;
		private string						mDatabaseLocation;
		private string						mDatabaseName;

		public	DB		Database { get; private set; }

		[ImportMany("PersistenceType")]
		public IEnumerable<Type>	PersistenceTypes;

		public DatabaseManager() {
			mLog = new Log();
		}

		public bool InitializeDatabase( string databaseLocation ) {
			var retValue = true;

			mDatabaseLocation = databaseLocation;

			try {
				Database = new DB( string.Format( "server={0};password=;options=none;", mDatabaseLocation ) );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );

				retValue = false;
			}

			Condition.Ensures( Database ).IsNotNull();
			return( retValue );
		}

		public void OpenWithCreateDatabase( string databaseName ) {
			mDatabaseName = databaseName;

			if(!OpenDatabase( mDatabaseName )) {
				CreateDatabase( mDatabaseName );
				OpenDatabase( mDatabaseName );

				RegisterDatabaseTypes();
				LoadDatabaseDefaults();
			}
		}

		public bool OpenDatabase( string databaseName ) {
			Condition.Requires( Database ).IsNotNull();

			var retValue = true;

			mDatabaseName = databaseName;

			try {
				Database.OpenDatabase( mDatabaseName );
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

		private void LoadDatabaseDefaults() {
			Condition.Requires( Database ).IsNotNull();

			var root = new RootFolder( @"D:\Music", "Music Storage" );

			root.FolderStrategy.SetStrategyForLevel( 0, eFolderStrategy.Artist );
			root.FolderStrategy.SetStrategyForLevel( 1, eFolderStrategy.Album );
			root.FolderStrategy.SetStrategyForLevel( 2, eFolderStrategy.Volume );
			root.FolderStrategy.PreferFolderStrategy = true;

			Database.Store( root );
		}
	}
}
