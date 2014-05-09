using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public void BackupLibrary( LibraryConfiguration library ) {
			try {
				var libraryBackup = mLibraryConfiguration.OpenLibraryBackup( library );

				try {
					var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

					if(!string.IsNullOrEmpty( databaseName )) {
						var backupDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
						var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

						if(!Directory.Exists( backupDatabasePath )) {
							Directory.CreateDirectory( backupDatabasePath );
						}

						mDatabaseUtility.BackupDatabase( databaseName, backupDatabaseName );

						mLibraryConfiguration.CloseLibraryBackup( library, libraryBackup );

						NoiseLogger.Current.LogMessage( "Backup of library '{0}' was completed ('{1}')", library.LibraryName, libraryBackup.BackupDate );
					}
				}
				catch( Exception ex ) {
					mLibraryConfiguration.AbortLibraryBackup( library, libraryBackup );

					NoiseLogger.Current.LogException( "LibrarianModel:BackupLibrary - Database backup failed.", ex );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarianModel:BackupLibrary - Could not open library backup.", ex );
			}
		}

		public void RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup ) {
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

						NoiseLogger.Current.LogMessage( "Restore of library '{0}' was completed ('{1} - {2}')",
														library.LibraryName, 
														libraryBackup.BackupDate.ToShortDateString(),
														libraryBackup.BackupDate.ToShortTimeString() );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarianModel:RestoreLibrary", ex );
			}
		}

		public void ExportLibrary( LibraryConfiguration library, string exportPath ) {
			
		}

		public void ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup ) {
			try {
				if( Directory.Exists( libraryBackup.BackupPath )) {
					var newLibrary = mLibraryConfiguration.OpenLibraryRestore( library, libraryBackup );
					var importDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
					var	databaseName = library.DatabaseName.Replace( ' ', '_' );
					var importDatabaseName = Directory.EnumerateFiles( importDatabasePath, "*" + Constants.Ef_DatabaseBackupExtension ).FirstOrDefault();

					if(!string.IsNullOrWhiteSpace( importDatabaseName )) {
						var restoreList = mDatabaseUtility.RestoreFileList( importDatabaseName );
						var fileList = new List<string>();
						var locationList = new List<string>();

						foreach( var file in restoreList ) {
							var fileName = Path.GetFileName( file.PhysicalName );

							if(!string.IsNullOrWhiteSpace( fileName )) {
								fileList.Add( fileName );
								locationList.Add( Path.Combine( newLibrary.LibraryDatabasePath, fileName ));
							}
						}

						mDatabaseUtility.RestoreDatabase( databaseName, importDatabaseName, fileList, locationList );
						mLibraryConfiguration.CloseLibraryRestore( newLibrary );

						NoiseLogger.Current.LogMessage( "Import of library '{0}' was completed.", library.LibraryName );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( string.Format( "LibrarianModel:ImportLibrary from '{0}'", libraryBackup.BackupPath ), ex );
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
