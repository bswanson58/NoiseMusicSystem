using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	internal class FileNameProvider {
		private readonly IDatabaseManager	mDatabase;
		private long						mFolderId;
		private List<StorageFile>			mFolderFiles;

		public FileNameProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			if( forFile.ParentFolder != mFolderId ) {
				BuildFolderFiles( forFile.ParentFolder );
			}

			return( new NameProvider( forFile, mFolderFiles ));
		}

		private void BuildFolderFiles( long parentId ) {
			var files = from StorageFile file in mDatabase.Database where file.ParentFolder == parentId orderby file.Name select file;

			mFolderFiles = new List<StorageFile>( files.ToList());
			mFolderId = parentId;
		}
	}

	internal class NameProvider : IMetaDataProvider {
		private readonly StorageFile				mFile;
		private readonly IList<StorageFile>	mFolderFiles;

		public NameProvider( StorageFile file, IList<StorageFile> folderFiles ) {
			mFolderFiles = folderFiles;
			mFile = file;
		}

		public string Artist {
			get{ return( "" ); }
		}

		public string Album {
			get{ return( "" ); }
		}

		public string TrackName {
			get {
				var	trackName = Path.GetFileNameWithoutExtension( mFile.Name );
				var nameParts = trackName.Split( new []{ '-' });

				return( nameParts.Count() > 1 ? nameParts[1].Trim() : trackName );
			}
		}

		public string VolumeName {
			get{ return( "" ); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			if( mFolderFiles != null ) {
				track.TrackNumber = (UInt16)( mFolderFiles.IndexOf( mFile ) + 1 );
			}
		}
	}
}
