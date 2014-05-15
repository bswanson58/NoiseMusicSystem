using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Noise.Librarian.Interfaces;

namespace Noise.Librarian.Models {
	public class DirectoryArchiver : IDirectoryArchiver {
		public void BackupDirectory( string sourcePath, string destinationName ) {
			using( var zipFile = new ZipFile( destinationName ) ) {
				zipFile.AddDirectory( sourcePath );
			}
		}

		public void RestoreDirectory( string sourceName, string destinationPath ) {
			using( var zipFile = ZipFile.Read( sourceName )) {
				zipFile.ExtractAll( destinationPath, ExtractExistingFileAction.OverwriteSilently );
			}
		}

		public void BackupSubdirectories( string sourcePath, string destinationPath, Action<ProgressReport> progressCallback ) {
			if( Directory.Exists( sourcePath )) {
				var directoryList = Directory.EnumerateDirectories( sourcePath ).ToArray();
				var perDirectoryMultiplier = (float)1000 / directoryList.Count();
				var progressBase = 0f;

				foreach( var directory in directoryList ) {
					var directoryName = Path.GetFileName( directory );
					var archiveName = Path.Combine( destinationPath, directoryName );

					using( var zipFile = new ZipFile( archiveName )) {
						if( progressCallback != null ) {
							var @base = progressBase;

							zipFile.SaveProgress += ( sender, args ) => {
								if( args.EventType == ZipProgressEventType.Saving_AfterWriteEntry ) {
									progressCallback( new ProgressReport( "Archiving Directory", directoryName,
																			(int)(( perDirectoryMultiplier * ((float)args.EntriesSaved / args.EntriesTotal )) + @base )));
								}
							};
						}

						zipFile.AddDirectory( directory, directoryName );

						zipFile.Save();
					}

					progressBase += perDirectoryMultiplier;
				}
			}
		}

		public void RestoreSubdirectories( string sourcePath, string destinationPath, Action<ProgressReport> progressCallback ) {
			if( Directory.Exists( sourcePath )) {
				var archiveList = Directory.EnumerateFiles( sourcePath ).ToArray();
				var perDirectoryMultiplier = (float)1000 / archiveList.Count();
				var progressBase = 0f;

				foreach( var archiveFile in archiveList ) {
					using( var zipFile = ZipFile.Read( archiveFile )) {
						if( progressCallback != null ) {
							var @base = progressBase;
							var @archive = Path.GetFileName( archiveFile );

							zipFile.ExtractProgress += ( sender, args ) => {
								if( args.EventType == ZipProgressEventType.Extracting_AfterExtractEntry ) {
									progressCallback( new ProgressReport( "Restoring Directory", @archive,
																			(int)(( perDirectoryMultiplier * ((float)args.EntriesExtracted / args.EntriesTotal )) + @base )));
								}
							};
						}

						zipFile.ExtractAll( destinationPath, ExtractExistingFileAction.OverwriteSilently );
					}

					progressBase += perDirectoryMultiplier;
				}
			}
		}
	}
}
