using System;
using System.IO;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Interfaces;

namespace Noise.Librarian.Models {
	public class LibrarianModel : ILibrarian {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILifecycleManager		mLifecycleManager;
		private readonly IDatabaseManager		mDatabaseManager;
		private readonly IDatabaseUtility		mDatabaseUtility;
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public LibrarianModel( IEventAggregator eventAggregator,
							   ILifecycleManager lifecycleManager,
							   IDatabaseManager databaseManager,
							   IDatabaseUtility databaseUtility,
							   ILibraryConfiguration libraryConfiguration ) {
			mEventAggregator = eventAggregator;
			mLifecycleManager = lifecycleManager;
			mDatabaseManager = databaseManager;
			mDatabaseUtility = databaseUtility;
			mLibraryConfiguration = libraryConfiguration;
		}

		public bool Initialize() {
			NoiseLogger.Current.LogMessage( "Initializing Noise Music System - Librarian" );

			try {
				mLifecycleManager.Initialize();

				NoiseLogger.Current.LogMessage( "Initialized LibrarianModel." );

				mEventAggregator.Publish( new Events.SystemInitialized());
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarianModel:Initialize", ex );
			}

			return( true );
		}

		public void BackupDatabase( LibraryConfiguration library ) {
			try {
				var backupDirectory = mLibraryConfiguration.OpenLibraryBackup( library );

				try {
					var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

					if(!string.IsNullOrEmpty( databaseName )) {
						var backupDatabasePath = Path.Combine( backupDirectory, Constants.LibraryDatabaseDirectory );
						var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

						if(!Directory.Exists( backupDatabasePath )) {
							Directory.CreateDirectory( backupDatabasePath );
						}

						mDatabaseUtility.BackupDatabase( databaseName, backupDatabaseName );

						mLibraryConfiguration.CloseLibraryBackup( library, backupDirectory );
					}
				}
				catch( Exception ex ) {
					mLibraryConfiguration.AbortLibraryBackup( library, backupDirectory );

					NoiseLogger.Current.LogException( "LibrarianModel:BackupDatabase - Database backup failed.", ex );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarianModel:BackupDatabase - Could not open library backup.", ex );
			}
		}

		public void Shutdown() {
			mEventAggregator.Publish( new Events.SystemShutdown());

			mLifecycleManager.Shutdown();
			mDatabaseManager.Shutdown();

			NoiseLogger.Current.LogMessage( "Shutdown LibrarianModel." );
		}
	}
}
