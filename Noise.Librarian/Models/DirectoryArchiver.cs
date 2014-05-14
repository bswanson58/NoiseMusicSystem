using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Ionic.Zip;
using Noise.Librarian.Interfaces;
using ZipFile = System.IO.Compression.ZipFile;

namespace Noise.Librarian.Models {
	public class DirectoryArchiver : IDirectoryArchiver {
		public void BackupDirectory( string sourcePath, string destinationName ) {
			ZipFile.CreateFromDirectory( sourcePath, destinationName, CompressionLevel.Optimal, false );
		}

		public void RestoreDirectory( string sourceName, string destinationPath ) {
			Compression.ImprovedExtractToDirectory( sourceName, destinationPath, Compression.Overwrite.Always );
		}

		public void BackupSubdirectories( string sourcePath, string destinationPath, Action<ProgressReport> progressCallback ) {
			if( Directory.Exists( sourcePath )) {
				var directoryList = Directory.EnumerateDirectories( sourcePath ).ToArray();
				var perDirectoryMultiplier = (float)1000 / directoryList.Count();
				var progressBase = 0f;

				foreach( var directory in directoryList ) {
					var directoryName = Path.GetFileName( directory );
					var archiveName = Path.Combine( destinationPath, directoryName );

					using( var zipFile = new Ionic.Zip.ZipFile( archiveName )) {
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

		public void RestoreSubdirectories( string sourcePath, string destinationPath ) {
			if( Directory.Exists( sourcePath )) {
				var archiveList = Directory.EnumerateFiles( sourcePath );

				foreach( var archiveFile in archiveList ) {
					Compression.ImprovedExtractToDirectory( archiveFile, destinationPath, Compression.Overwrite.Always );
				}
			}
		}
	}
}
