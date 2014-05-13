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
		private readonly IMetadataManager		mMetadataManager;
		private readonly IDirectoryArchiver		mDirectoryArchiver;
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public LibrarianModel( IEventAggregator eventAggregator,
							   ILifecycleManager lifecycleManager,
							   IDatabaseManager databaseManager,
							   IDatabaseUtility databaseUtility,
							   IMetadataManager metadataManager,
							   IDirectoryArchiver directoryArchiver,
							   ILibraryConfiguration libraryConfiguration ) {
			mEventAggregator = eventAggregator;
			mLifecycleManager = lifecycleManager;
			mDatabaseManager = databaseManager;
			mDatabaseUtility = databaseUtility;
			mMetadataManager = metadataManager;
			mDirectoryArchiver = directoryArchiver;
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
						var backupPath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
						var backupName = Path.Combine( backupPath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}

						mDatabaseUtility.BackupDatabase( databaseName, backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory );
						backupName = Path.Combine( backupPath, Constants.MetadataBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}

						mMetadataManager.ExportMetadata( backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory );
						backupName = Path.Combine( backupPath, Constants.SearchDatabaseBackupName );
						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mDirectoryArchiver.BackupDirectory( library.SearchDatabasePath, backupName );

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
					
					mLibraryConfiguration.OpenLibraryRestore( library, libraryBackup );

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

						var backupMetadataName = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory, Constants.MetadataBackupName );
						
						if( File.Exists( backupMetadataName )) {
							mMetadataManager.ImportMetadata( backupMetadataName );
						}

						var backupSearchName = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory, Constants.SearchDatabaseBackupName );

						if( File.Exists( backupSearchName )) {
							mDirectoryArchiver.RestoreDirectory( backupSearchName, library.SearchDatabasePath );
						}

						mLibraryConfiguration.CloseLibraryRestore( library, libraryBackup );

						NoiseLogger.Current.LogMessage( "Restore of library '{0}' was completed ('{1} - {2}')",
														library.LibraryName, 
														libraryBackup.BackupDate.ToShortDateString(),
														libraryBackup.BackupDate.ToShortTimeString() );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarianModel:RestoreLibrary", ex );

				mLibraryConfiguration.AbortLibraryRestore( library, libraryBackup );
			}
		}

		public void ExportLibrary( LibraryConfiguration library, string exportPath ) {
			try {
				var libraryBackup = mLibraryConfiguration.OpenLibraryExport( library, exportPath );

				try {
					var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

					if(!string.IsNullOrEmpty( databaseName )) {
						var backupDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
						var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

						if(!Directory.Exists( backupDatabasePath )) {
							Directory.CreateDirectory( backupDatabasePath );
						}

						mDatabaseUtility.BackupDatabase( databaseName, backupDatabaseName );

						var backupPath = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory );
						var backupName = Path.Combine( backupPath, Constants.MetadataBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
		
						mMetadataManager.ExportMetadata( backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory );
						backupName = Path.Combine( backupPath, Constants.SearchDatabaseBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mDirectoryArchiver.BackupDirectory( library.SearchDatabasePath, backupName );

						mLibraryConfiguration.CloseLibraryExport( library, libraryBackup );

						NoiseLogger.Current.LogMessage( "Export of library '{0}' was completed to ('{1}')", library.LibraryName, libraryBackup.BackupPath );
					}
				}
				catch( Exception ex ) {
					mLibraryConfiguration.AbortLibraryExport( library, libraryBackup );

					NoiseLogger.Current.LogException( "LibrarianModel:BackupLibrary - Database backup failed.", ex );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogMessage( "LibrarianModel:ExportLibrary", ex );
			}
		}

		public void ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup ) {
			try {
				if( Directory.Exists( libraryBackup.BackupPath )) {
					var newLibrary = mLibraryConfiguration.OpenLibraryImport( library, libraryBackup );
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
		
						var backupMetadataName = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory, Constants.MetadataBackupName );
						
						if( File.Exists( backupMetadataName )) {
							mMetadataManager.ImportMetadata( backupMetadataName );
						}

						var backupSearchName = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory, Constants.SearchDatabaseBackupName );

						if( File.Exists( backupSearchName )) {
							mDirectoryArchiver.RestoreDirectory( backupSearchName, newLibrary.SearchDatabasePath );
						}

						mLibraryConfiguration.CloseLibraryImport( newLibrary );

						NoiseLogger.Current.LogMessage( "Import of library '{0}' was completed.", library.LibraryName );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( string.Format( "LibrarianModel:ImportLibrary from '{0}'", libraryBackup.BackupPath ), ex );

				mLibraryConfiguration.AbortLibraryImport( library );
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
