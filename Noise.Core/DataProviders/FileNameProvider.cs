using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eloquera.Linq;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	internal class FileNameProvider {
		private readonly IDatabaseManager	mDatabase;

		public FileNameProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			return( new NameProvider( mDatabase, forFile ));
		}
	}

	internal class NameProvider : IMetaDataProvider {
		private readonly IDatabaseManager	mDatabase;
		private readonly StorageFile		mFile;
		private long						mFolderId;
		private List<StorageFile>			mFolderFiles;

		public NameProvider( IDatabaseManager databaseManager, StorageFile file ) {
			mDatabase = databaseManager;
			mFile = file;

			mFolderId = Constants.cDatabaseNullOid;
		}

		public string Artist {
			get{ return( "" ); }
		}

		public string Album {
			get{ return( "" ); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			if( mFile.ParentFolder != mFolderId ) {
				BuildFolderFiles( mFile.ParentFolder );
			}

			track.TrackNumber = (UInt16)( mFolderFiles.IndexOf( mFile ) + 1 );

			var	trackName = Path.GetFileNameWithoutExtension( mFile.Name );
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
