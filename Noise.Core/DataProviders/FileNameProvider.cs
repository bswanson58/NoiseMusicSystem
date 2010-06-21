using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eloquera.Linq;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public class FileNameProvider {
		private readonly IDatabaseManager	mDatabase;
		private long						mFolderId;
		private List<StorageFile>			mFolderFiles;

		public FileNameProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;

			mFolderId = Constants.cDatabaseNullOid;
		}

		public void BuildMetaData( StorageFile storageFile, DbTrack track ) {
			if( storageFile.ParentFolder != mFolderId ) {
				BuildFolderFiles( storageFile.ParentFolder );
			}

			track.TrackNumber = (UInt16)( mFolderFiles.IndexOf( storageFile ) + 1 );

			var	trackName = Path.GetFileNameWithoutExtension( storageFile.Name );
			var nameParts = trackName.Split( new []{ '-' });
			track.Name = nameParts.Last().Trim();
		}

		private void BuildFolderFiles( long parentId ) {
			var files = from StorageFile file in mDatabase.Database where file.ParentFolder == parentId orderby file.Name select file;

			mFolderFiles = new List<StorageFile>( files.ToList());
			mFolderId = parentId;
		}
	}
}
