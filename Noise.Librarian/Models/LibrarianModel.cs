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

						NoiseLogger.Current.LogInfo( "Backup of library '{0}' was completed ('{1}')", library.LibraryName, Path.GetDirectoryName( backupDirectory ));
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

		public void RestoreDatabase( LibraryConfiguration library, LibraryBackup libraryBackup ) {
			try {
				if( Directory.Exists( libraryBackup.BackupPath )) {
					var backupDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
					var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

					if( File.Exists( backupDatabaseName )) {
						var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

						if( !string.IsNullOrWhiteSpace( databaseName )) {
							mDatabaseUtility.DetachDatabase( databaseName );
						}
						else {
							databaseName = library.DatabaseName.Replace( ' ', '_' );
						}

						var files = mDatabaseUtility.RestoreFileList( backupDatabaseName );

						foreach( var file in files ) {
							var path = Path.GetDirectoryName( file.PhysicalName );

							if((!string.IsNullOrWhiteSpace( path )) &&
							   (!Directory.Exists( path ))) {
								Directory.CreateDirectory( path );
							}
						}

						mDatabaseUtility.RestoreDatabase( databaseName, backupDatabaseName );

						NoiseLogger.Current.LogInfo( "Restore of library '{0}' was completed ('{1} - {2}')",
														library.LibraryName, 
														libraryBackup.BackupDate.ToShortDateString(),
														libraryBackup.BackupDate.ToShortTimeString() );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarianModel:RestoreDatabase", ex );
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
