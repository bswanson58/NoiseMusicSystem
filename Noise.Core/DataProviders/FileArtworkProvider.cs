using System;
using System.IO;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class FileArtworkProvider {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;

		public FileArtworkProvider( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public void BuildMetaData( StorageFile file ) {
			var dbManager = mContainer.Resolve<IDatabaseManager>();
			var database = dbManager.ReserveDatabase( "FileArtworkProvider" );

			try {
				var fileId = file.DbId;
				var dbPicture = new DbArtwork( fileId, IsCoverFile( file.Name ) ? ContentType.AlbumCover : ContentType.AlbumArtwork )
					{ Source = InfoSource.File,
					  FolderLocation = file.ParentFolder };
				var	fileName = StorageHelpers.GetPath( database.Database, file );

				dbPicture.Image = File.ReadAllBytes( fileName );

				database.Database.Store( dbPicture );
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "FileArtworkProvider file: {0}", StorageHelpers.GetPath( database.Database, file )), ex );
			}
			finally {
				dbManager.FreeDatabase( database.DatabaseId );
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
