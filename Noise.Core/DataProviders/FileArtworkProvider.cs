using System;
using System.IO;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	internal class FileArtworkProvider {
		private readonly IDatabaseManager	mDatabase;

		public FileArtworkProvider( IDatabaseManager database ) {
			mDatabase =database;
		}

		public void BuildMetaData( StorageFile file ) {
			var fileId = mDatabase.Database.GetUid( file );
			var dbPicture = new DbArtwork( fileId ) { ArtworkType = IsCoverFile( file.Name ) ? ArtworkTypes.AlbumCover : ArtworkTypes.AlbumOther,
													  Source = InfoSource.File,
													  FolderLocation = file.ParentFolder };

			try {
				var	fileName = StorageHelpers.GetPath( mDatabase.Database, file );

				dbPicture.Image = File.ReadAllBytes( fileName );

				mDatabase.Database.Store( dbPicture );
			}
			catch( Exception ex ) {
				
			}
		}

		private static bool IsCoverFile( string fileName ) {
			var retValue = false;
			var name = Path.GetFileNameWithoutExtension( fileName ).ToLower();

			if(( name.Equals( "albumartsmall" )) ||
			   ( name.Equals( "cover" )) ||
			   ( name.Equals( "folder" )) ||
			   ( name.Equals( "front" ))) {
				retValue = true;
			}

			return( retValue );
		}
	}
}
