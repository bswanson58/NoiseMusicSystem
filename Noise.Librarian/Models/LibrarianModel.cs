using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Interfaces;
using ReusableBits;

namespace Noise.Librarian.Models {
	public class LibrarianModel : ILibrarian {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILifecycleManager		mLifecycleManager;
		private readonly IDatabaseManager		mDatabaseManager;
		private readonly IDatabaseUtility		mDatabaseUtility;
		private readonly IMetadataManager		mMetadataManager;
		private readonly IDirectoryArchiver		mDirectoryArchiver;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly INoiseLog				mLog;
		private TaskHandler						mBackupTaskHandler;
		private TaskHandler						mRestoreTaskHandler;
		private TaskHandler						mExportTaskHandler;
		private TaskHandler						mImportTaskHandler;

		public LibrarianModel( IEventAggregator eventAggregator,
							   ILifecycleManager lifecycleManager,
							   IDatabaseManager databaseManager,
							   IDatabaseUtility databaseUtility,
							   IMetadataManager metadataManager,
							   IDirectoryArchiver directoryArchiver,
							   ILibraryConfiguration libraryConfiguration,
							   INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mLifecycleManager = lifecycleManager;
			mDatabaseManager = databaseManager;
			mDatabaseUtility = databaseUtility;
			mMetadataManager = metadataManager;
			mDirectoryArchiver = directoryArchiver;
			mLibraryConfiguration = libraryConfiguration;
			mLog = log;
		}

		public bool Initialize() {
			mLog.LogMessage( "Initializing Noise Music System - Librarian" );

			try {
				mLifecycleManager.Initialize();

				mLog.LogMessage( "Initialized LibrarianModel." );

				mEventAggregator.Publish( new Events.SystemInitialized());
			}
			catch( Exception ex ) {
				mLog.LogException( "Failed to Initialize", ex );
			}

			return( true );
		}

		protected TaskHandler BackupTaskHandler {
			get {
				if( mBackupTaskHandler == null ) {
					Execute.OnUIThread( () => mBackupTaskHandler = new TaskHandler());
				}

				return( mBackupTaskHandler );
			}
			set {  mBackupTaskHandler = value; }
		}

		public void BackupLibrary( LibraryConfiguration library, Action<ProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				BackupTaskHandler.StartTask( () => BackupLibraryTask( library, progressCallback ),
											 () => progressCallback( new ProgressReport( "Backup Completed - Success", library.LibraryName )),
											 error => progressCallback( new ProgressReport( "Backup Failed.", library.LibraryName )));
			}
		}

		private void BackupLibraryTask( LibraryConfiguration library, Action<ProgressReport> progressCallback ) {
			try {
				var libraryBackup = mLibraryConfiguration.OpenLibraryBackup( library );

				try {
					var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

					progressCallback( new ProgressReport( "Starting Library backup", library.DatabaseName, 0 ));

					if(!string.IsNullOrEmpty( databaseName )) {
						var backupPath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
						var backupName = Path.Combine( backupPath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}

						progressCallback( new ProgressReport( "Starting database backup", library.DatabaseName, 250 ));
						mDatabaseUtility.BackupDatabase( databaseName, backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory );
						backupName = Path.Combine( backupPath, Constants.MetadataBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}

						progressCallback( new ProgressReport( "Starting Metadata backup", library.LibraryName, 500 ));
						mMetadataManager.ExportMetadata( backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory );
						backupName = Path.Combine( backupPath, Constants.SearchDatabaseBackupName );
						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						progressCallback( new ProgressReport( "Starting Search Database backup", library.LibraryName, 750 ));
						mDirectoryArchiver.BackupDirectory( library.SearchDatabasePath, backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );
						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mDirectoryArchiver.BackupSubdirectories( library.BlobDatabasePath, backupPath, progressCallback );

						mLibraryConfiguration.CloseLibraryBackup( library, libraryBackup );

						mLog.LogMessage( string.Format( "Backup of {0} was completed ('{1}')", library, libraryBackup.BackupDate ));
					}
				}
				catch( Exception ex ) {
					mLibraryConfiguration.AbortLibraryBackup( library, libraryBackup );

					progressCallback( new ProgressReport( "Completed Library backup.", "Failed" ));
					mLog.LogException( string.Format( "Database backup failed. {0}", library ), ex );
				}
			}
			catch( Exception ex ) {
				progressCallback( new ProgressReport( "Completed Library backup.", "Failed" ));
				mLog.LogException( string.Format( "Could not open library backup. {0}", library ), ex );
			}
		}

		protected TaskHandler RestoreTaskHandler {
			get {
				if( mRestoreTaskHandler == null ) {
					Execute.OnUIThread( () => mRestoreTaskHandler = new TaskHandler());
				}

				return( mRestoreTaskHandler );
			}
			set {  mRestoreTaskHandler = value; }
		}

		public void RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<ProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				RestoreTaskHandler.StartTask( () => RestoreLibraryTask( library, libraryBackup, progressCallback ),
											 () => progressCallback( new ProgressReport( "Restore Completed - Success", library.LibraryName )),
											 error => progressCallback( new ProgressReport( "Restore Failed.", library.LibraryName )));
			}
		}

		private void RestoreLibraryTask( LibraryConfiguration library, LibraryBackup libraryBackup, Action<ProgressReport> progressCallback ) {
			try {
				if( Directory.Exists( libraryBackup.BackupPath )) {
					var backupDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
					var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );
					
					progressCallback( new ProgressReport( "Starting Library restore.", library.LibraryName, 0 ));
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

						progressCallback( new ProgressReport( "Starting Database restore.", library.DatabaseName, 250 ));
						mDatabaseUtility.RestoreDatabase( databaseName, backupDatabaseName );

						var backupMetadataName = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory, Constants.MetadataBackupName );
						
						if( File.Exists( backupMetadataName )) {
							mMetadataManager.ImportMetadata( backupMetadataName );
						}

						var backupSearchName = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory, Constants.SearchDatabaseBackupName );

						if( File.Exists( backupSearchName )) {
						progressCallback( new ProgressReport( "Starting Search Database restore.", library.LibraryName, 500 ));

							mDirectoryArchiver.RestoreDirectory( backupSearchName, library.SearchDatabasePath );
						}

						var backupBlobPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );
						mDirectoryArchiver.RestoreSubdirectories( backupBlobPath, library.BlobDatabasePath, progressCallback );

						mLibraryConfiguration.CloseLibraryRestore( library, libraryBackup );

						mLog.LogMessage( string.Format( "Restore of {0} was completed ('{1} - {2}')", library,
														libraryBackup.BackupDate.ToShortDateString(), libraryBackup.BackupDate.ToShortTimeString()));
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Restoring {0}", library ), ex );

				progressCallback( new ProgressReport( "Completed Library restore.", "Failed" ));
				mLibraryConfiguration.AbortLibraryRestore( library, libraryBackup );
			}
		}

		protected TaskHandler ExportTaskHandler {
			get {
				if( mExportTaskHandler == null ) {
					Execute.OnUIThread( () => mExportTaskHandler = new TaskHandler());
				}

				return( mExportTaskHandler );
			}
			set {  mExportTaskHandler = value; }
		}

		public void ExportLibrary( LibraryConfiguration library, string exportPath, Action<ProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				ExportTaskHandler.StartTask( () => ExportLibraryTask( library, exportPath, progressCallback ),
											 () => progressCallback( new ProgressReport( "Export Completed - Success", library.LibraryName )),
											 error => progressCallback( new ProgressReport( "Export Failed.", library.LibraryName )));
			}
		}

		private void ExportLibraryTask( LibraryConfiguration library, string exportPath, Action<ProgressReport> progressCallback ) {
			try {
				var libraryBackup = mLibraryConfiguration.OpenLibraryExport( library, exportPath );

				try {
					var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

					progressCallback( new ProgressReport( "Starting Library export", library.LibraryName, 0 ));

					if(!string.IsNullOrEmpty( databaseName )) {
						var backupDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
						var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

						if(!Directory.Exists( backupDatabasePath )) {
							Directory.CreateDirectory( backupDatabasePath );
						}

						progressCallback( new ProgressReport( "Starting database export", library.DatabaseName, 0 ));
						mDatabaseUtility.BackupDatabase( databaseName, backupDatabaseName );

						var backupPath = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory );
						var backupName = Path.Combine( backupPath, Constants.MetadataBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
		
						progressCallback( new ProgressReport( "Starting Metadata export", library.LibraryName, 0 ));
						mMetadataManager.ExportMetadata( backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory );
						backupName = Path.Combine( backupPath, Constants.SearchDatabaseBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}

						progressCallback( new ProgressReport( "Starting Search Database export", library.LibraryName, 0 ));
						mDirectoryArchiver.BackupDirectory( library.SearchDatabasePath, backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );
						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mDirectoryArchiver.BackupSubdirectories( library.BlobDatabasePath, backupPath, progressCallback );

						mLibraryConfiguration.CloseLibraryExport( library, libraryBackup );

						mLog.LogMessage( string.Format( "Export of {0} was completed to ('{1}')", library, libraryBackup.BackupPath ));
					}
				}
				catch( Exception ex ) {
					mLibraryConfiguration.AbortLibraryExport( library, libraryBackup );

					progressCallback( new ProgressReport( "Export library failed.", library.LibraryName ));
					mLog.LogException( string.Format( "Export library failed. {0}", library ), ex );
				}
			}
			catch( Exception ex ) {
				progressCallback( new ProgressReport( "Export library failed.", library.LibraryName ));
				mLog.LogException( string.Format( "Export of {0}", library ), ex );
			}
		}

		protected TaskHandler ImportTaskHandler {
			get {
				if( mImportTaskHandler == null ) {
					Execute.OnUIThread( () => mImportTaskHandler = new TaskHandler());
				}

				return( mImportTaskHandler );
			}
			set {  mImportTaskHandler = value; }
		}

		public void ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<ProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				ImportTaskHandler.StartTask( () => ImportLibraryTask( library, libraryBackup, progressCallback ),
											 () => progressCallback( new ProgressReport( "Import Completed - Success", library.LibraryName )),
											 error => progressCallback( new ProgressReport( "Import Failed.", library.LibraryName )));
			}
		}

		private void ImportLibraryTask( LibraryConfiguration library, LibraryBackup libraryBackup, Action<ProgressReport> progressCallback ) {
			try {
				if( Directory.Exists( libraryBackup.BackupPath )) {
					var newLibrary = mLibraryConfiguration.OpenLibraryImport( library, libraryBackup );
					var importDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
					var	databaseName = library.DatabaseName.Replace( ' ', '_' );
					var importDatabaseName = Directory.EnumerateFiles( importDatabasePath, "*" + Constants.Ef_DatabaseBackupExtension ).FirstOrDefault();

					progressCallback( new ProgressReport( "Starting Library import.", library.LibraryName, 0 ));
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

						progressCallback( new ProgressReport( "Starting database import.", library.DatabaseName, 250 ));
						mDatabaseUtility.RestoreDatabase( databaseName, importDatabaseName, fileList, locationList );
		
						var backupMetadataName = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory, Constants.MetadataBackupName );
						
						if( File.Exists( backupMetadataName )) {
							progressCallback( new ProgressReport( "Starting Metadata import.", library.LibraryName, 500 ));

							mMetadataManager.ImportMetadata( backupMetadataName );
						}

						var backupSearchName = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory, Constants.SearchDatabaseBackupName );

						if( File.Exists( backupSearchName )) {
							progressCallback( new ProgressReport( "Starting Search Database import.", library.LibraryName, 750 ));

							mDirectoryArchiver.RestoreDirectory( backupSearchName, newLibrary.SearchDatabasePath );
						}

						var backupBlobPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );
						mDirectoryArchiver.RestoreSubdirectories( backupBlobPath, newLibrary.BlobDatabasePath, progressCallback );

						mLibraryConfiguration.CloseLibraryImport( newLibrary );

						mLog.LogMessage( string.Format( "Imported {0}", library ));
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Import library from '{0}'", libraryBackup.BackupPath ), ex );

				progressCallback( new ProgressReport( "Completed Library import - Failed", library.LibraryName ));
				mLibraryConfiguration.AbortLibraryImport( library );
			}
		}

		public void Shutdown() {
			mEventAggregator.Publish( new Events.SystemShutdown());

			mLifecycleManager.Shutdown();
			mDatabaseManager.Shutdown();

			mLog.LogMessage( "Shutdown LibrarianModel." );
		}
	}
}
