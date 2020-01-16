using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;

namespace Noise.Core.Database {
	public class LibrarianModel : ILibrarian {
		private readonly IDatabaseUtility		mDatabaseUtility;
		private readonly IMetadataManager		mMetadataManager;
		private readonly IDirectoryArchiver		mDirectoryArchiver;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly INoiseLog				mLog;
		private TaskHandler						mBackupTaskHandler;
		private TaskHandler						mRestoreTaskHandler;
		private TaskHandler						mExportTaskHandler;
		private TaskHandler						mImportTaskHandler;

		public LibrarianModel( IDatabaseUtility databaseUtility,
							   IMetadataManager metadataManager,
							   IDirectoryArchiver directoryArchiver,
							   ILibraryConfiguration libraryConfiguration,
							   INoiseLog log ) {
			mDatabaseUtility = databaseUtility;
			mMetadataManager = metadataManager;
			mDirectoryArchiver = directoryArchiver;
			mLibraryConfiguration = libraryConfiguration;
			mLog = log;
		}

		protected TaskHandler BackupTaskHandler {
			get {
				if( mBackupTaskHandler == null ) {
					Execute.OnUIThread( () => mBackupTaskHandler = new TaskHandler());
				}

				return( mBackupTaskHandler );
			}
			set => mBackupTaskHandler = value;
        }

		public void BackupLibrary( LibraryConfiguration library, Action<LibrarianProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				BackupTaskHandler.StartTask( () => BackupLibraryTask( library, progressCallback ),
											 () => { },
											 error => progressCallback( new LibrarianProgressReport( "Backup Failed.", library.LibraryName )));
			}
		}

		public Task BackupLibraryAsync( LibraryConfiguration configuration, Action<LibrarianProgressReport> progressCallback ) {
			return Task.Run( () => {
				BackupLibraryTask( configuration, progressCallback );
            });
        }

		private void BackupLibraryTask( LibraryConfiguration library, Action<LibrarianProgressReport> progressCallback ) {
			try {
				var libraryBackup = mLibraryConfiguration.OpenLibraryBackup( library );

				try {
					var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

					progressCallback( new LibrarianProgressReport( "Starting Library backup", library.DatabaseName, 0 ));

					if(!string.IsNullOrEmpty( databaseName )) {
						var backupPath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
						var backupName = Path.Combine( backupPath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

                        progressCallback( new LibrarianProgressReport( "Starting database backup", library.DatabaseName, 250 ));

                        if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mDatabaseUtility.BackupDatabase( databaseName, backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory );
						backupName = Path.Combine( backupPath, Constants.MetadataBackupName );

                        progressCallback( new LibrarianProgressReport( "Starting Metadata backup", library.LibraryName, 500 ));

                        if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mMetadataManager.ExportMetadata( backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory );
						backupName = Path.Combine( backupPath, Constants.SearchDatabaseBackupName );

                        progressCallback( new LibrarianProgressReport( "Starting Search Database backup", library.LibraryName, 750 ));

                        if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mDirectoryArchiver.BackupDirectory( library.SearchDatabasePath, backupName );

						if(!library.IsMetadataInPlace ) {
                            backupPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );

                            progressCallback( new LibrarianProgressReport( "Starting Blob Database backup", library.LibraryName, 800 ));

                            if(!Directory.Exists( backupPath )) {
                                Directory.CreateDirectory( backupPath );
                            }
                            mDirectoryArchiver.BackupSubdirectories( library.BlobDatabasePath, backupPath, progressCallback );
                        }

						mLibraryConfiguration.CloseLibraryBackup( library, libraryBackup );

						mLog.LogMessage( $"Backup of {library} was completed ('{libraryBackup.BackupDate}')" );
                        progressCallback( new LibrarianProgressReport( "Backup Completed - Success", library.LibraryName ));
                    }
				}
				catch( Exception ex ) {
					mLibraryConfiguration.AbortLibraryBackup( library, libraryBackup );

					progressCallback( new LibrarianProgressReport( "Completed Library backup.", "Failed" ));
					mLog.LogException( $"Database backup failed. {library}", ex );
				}
			}
			catch( Exception ex ) {
				progressCallback( new LibrarianProgressReport( "Completed Library backup.", "Failed" ));
				mLog.LogException( $"Could not open library backup. {library}", ex );
			}
		}

		protected TaskHandler RestoreTaskHandler {
			get {
				if( mRestoreTaskHandler == null ) {
					Execute.OnUIThread( () => mRestoreTaskHandler = new TaskHandler());
				}

				return( mRestoreTaskHandler );
			}
			set => mRestoreTaskHandler = value;
        }

		public void RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				RestoreTaskHandler.StartTask( () => RestoreLibraryTask( library, libraryBackup, progressCallback ),
											 () => { },
											 error => progressCallback( new LibrarianProgressReport( "Restore Failed.", library.LibraryName )));
			}
		}

		private void RestoreLibraryTask( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback ) {
			try {
				if( Directory.Exists( libraryBackup.BackupPath )) {
					var backupDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
					var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );
					
					progressCallback( new LibrarianProgressReport( "Starting Library restore.", library.LibraryName, 0 ));
					mLibraryConfiguration.OpenLibraryRestore( library, libraryBackup );

					if( File.Exists( backupDatabaseName )) {
						var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

						if( !string.IsNullOrWhiteSpace( databaseName )) {
							mDatabaseUtility.DetachDatabase( new DatabaseInfo( databaseName ));
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

						progressCallback( new LibrarianProgressReport( "Starting Database restore.", library.DatabaseName, 250 ));
						mDatabaseUtility.RestoreDatabase( databaseName, backupDatabaseName );

						var backupMetadataName = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory, Constants.MetadataBackupName );
						
						if( File.Exists( backupMetadataName )) {
							mMetadataManager.ImportMetadata( backupMetadataName );
						}

						var backupSearchName = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory, Constants.SearchDatabaseBackupName );

						if( File.Exists( backupSearchName )) {
						    progressCallback( new LibrarianProgressReport( "Starting Search Database restore.", library.LibraryName, 500 ));

							mDirectoryArchiver.RestoreDirectory( backupSearchName, library.SearchDatabasePath );
						}

						if(!library.IsMetadataInPlace ) {
                            var backupBlobPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );

                            progressCallback( new LibrarianProgressReport( "Starting Blob Database restore.", library.LibraryName, 600 ));

                            mDirectoryArchiver.RestoreSubdirectories( backupBlobPath, library.BlobDatabasePath, progressCallback );
                        }

						mLibraryConfiguration.CloseLibraryRestore( library, libraryBackup );

                        progressCallback( new LibrarianProgressReport(  "Restore Completed - Success", library.LibraryName ));
						mLog.LogMessage( $"Restore of {library} was completed ('{libraryBackup.BackupDate.ToShortDateString()} - {libraryBackup.BackupDate.ToShortTimeString()}')" );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( $"Restoring {library}", ex );

				progressCallback( new LibrarianProgressReport( "Completed Library restore.", "Failed" ));
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
			set => mExportTaskHandler = value;
        }

		public void ExportLibrary( LibraryConfiguration library, string exportPath, Action<LibrarianProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				ExportTaskHandler.StartTask( () => ExportLibraryTask( library, exportPath, progressCallback ),
											 () => progressCallback( new LibrarianProgressReport( "Export Completed - Success", library.LibraryName )),
											 error => progressCallback( new LibrarianProgressReport( "Export Failed.", library.LibraryName )));
			}
		}

		private void ExportLibraryTask( LibraryConfiguration library, string exportPath, Action<LibrarianProgressReport> progressCallback ) {
			try {
				var libraryBackup = mLibraryConfiguration.OpenLibraryExport( library, exportPath );

				try {
					var databaseName = mDatabaseUtility.GetDatabaseName( library.DatabaseName );

					progressCallback( new LibrarianProgressReport( "Starting Library export", library.LibraryName, 0 ));

					if(!string.IsNullOrEmpty( databaseName )) {
						var backupDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
						var backupDatabaseName = Path.Combine( backupDatabasePath, library.DatabaseName + Constants.Ef_DatabaseBackupExtension );

						if(!Directory.Exists( backupDatabasePath )) {
							Directory.CreateDirectory( backupDatabasePath );
						}

						progressCallback( new LibrarianProgressReport( "Starting database export", library.DatabaseName, 0 ));
						mDatabaseUtility.BackupDatabase( databaseName, backupDatabaseName );

						var backupPath = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory );
						var backupName = Path.Combine( backupPath, Constants.MetadataBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
		
						progressCallback( new LibrarianProgressReport( "Starting Metadata export", library.LibraryName, 0 ));
						mMetadataManager.ExportMetadata( backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory );
						backupName = Path.Combine( backupPath, Constants.SearchDatabaseBackupName );

						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}

						progressCallback( new LibrarianProgressReport( "Starting Search Database export", library.LibraryName, 0 ));
						mDirectoryArchiver.BackupDirectory( library.SearchDatabasePath, backupName );

						backupPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );
						if(!Directory.Exists( backupPath )) {
							Directory.CreateDirectory( backupPath );
						}
						mDirectoryArchiver.BackupSubdirectories( library.BlobDatabasePath, backupPath, progressCallback );

						mLibraryConfiguration.CloseLibraryExport( library, libraryBackup );

						mLog.LogMessage( $"Export of {library} was completed to ('{libraryBackup.BackupPath}')" );
					}
				}
				catch( Exception ex ) {
					mLibraryConfiguration.AbortLibraryExport( library, libraryBackup );

					progressCallback( new LibrarianProgressReport( "Export library failed.", library.LibraryName ));
					mLog.LogException( $"Export library failed. {library}", ex );
				}
			}
			catch( Exception ex ) {
				progressCallback( new LibrarianProgressReport( "Export library failed.", library.LibraryName ));
				mLog.LogException( $"Export of {library}", ex );
			}
		}

		protected TaskHandler ImportTaskHandler {
			get {
				if( mImportTaskHandler == null ) {
					Execute.OnUIThread( () => mImportTaskHandler = new TaskHandler());
				}

				return( mImportTaskHandler );
			}
			set => mImportTaskHandler = value;
        }

		public void ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback ) {
			if(( library != null ) &&
			   ( progressCallback != null )) {
				ImportTaskHandler.StartTask( () => ImportLibraryTask( library, libraryBackup, progressCallback ),
											 () => progressCallback( new LibrarianProgressReport( "Import Completed - Success", library.LibraryName )),
											 error => progressCallback( new LibrarianProgressReport( "Import Failed.", library.LibraryName )));
			}
		}

		private void ImportLibraryTask( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback ) {
			try {
				if( Directory.Exists( libraryBackup.BackupPath )) {
					var newLibrary = mLibraryConfiguration.OpenLibraryImport( library, libraryBackup );
					var importDatabasePath = Path.Combine( libraryBackup.BackupPath, Constants.LibraryDatabaseDirectory );
					var	databaseName = library.DatabaseName.Replace( ' ', '_' );
					var importDatabaseName = Directory.EnumerateFiles( importDatabasePath, "*" + Constants.Ef_DatabaseBackupExtension ).FirstOrDefault();

					progressCallback( new LibrarianProgressReport( "Starting Library import.", library.LibraryName, 0 ));
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

						progressCallback( new LibrarianProgressReport( "Starting database import.", library.DatabaseName, 250 ));
						mDatabaseUtility.RestoreDatabase( databaseName, importDatabaseName, fileList, locationList );
		
						var backupMetadataName = Path.Combine( libraryBackup.BackupPath, Constants.MetadataDirectory, Constants.MetadataBackupName );
						
						if( File.Exists( backupMetadataName )) {
							progressCallback( new LibrarianProgressReport( "Starting Metadata import.", library.LibraryName, 500 ));

							mMetadataManager.ImportMetadata( backupMetadataName );
						}

						var backupSearchName = Path.Combine( libraryBackup.BackupPath, Constants.SearchDatabaseDirectory, Constants.SearchDatabaseBackupName );

						if( File.Exists( backupSearchName )) {
							progressCallback( new LibrarianProgressReport( "Starting Search Database import.", library.LibraryName, 750 ));

							mDirectoryArchiver.RestoreDirectory( backupSearchName, newLibrary.SearchDatabasePath );
						}

						var backupBlobPath = Path.Combine( libraryBackup.BackupPath, Constants.BlobDatabaseDirectory );
						mDirectoryArchiver.RestoreSubdirectories( backupBlobPath, newLibrary.BlobDatabasePath, progressCallback );

						mLibraryConfiguration.CloseLibraryImport( newLibrary );

						mLog.LogMessage( $"Imported {library}" );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( $"Import library from '{libraryBackup.BackupPath}'", ex );

				progressCallback( new LibrarianProgressReport( "Completed Library import - Failed", library.LibraryName ));
				mLibraryConfiguration.AbortLibraryImport( library );
			}
		}
	}
}
