using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class FileNameProvider {
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;
		private long						mFolderId;
		private List<StorageFile>			mFolderFiles;

		public FileNameProvider( IUnityContainer container ) {
			mDatabaseManager = container.Resolve<IDatabaseManager>();
			mLog = container.Resolve<ILog>();
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			if( forFile.ParentFolder != mFolderId ) {
				BuildFolderFiles( forFile.ParentFolder );
			}

			return( new NameProvider( forFile, mFolderFiles ));
		}

		private void BuildFolderFiles( long parentId ) {
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var files = from StorageFile file in database.Database
							where ( file.ParentFolder == parentId ) && ( StorageHelpers.DetermineFileType( file ) == eFileType.Music )
							orderby file.Name select file;

				mFolderFiles = new List<StorageFile>( files.ToList());
				mFolderId = parentId;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - FileNameProvider", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}
	}

	internal class NameProvider : IMetaDataProvider {
		private readonly StorageFile		mFile;
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
				var listTrack = mFolderFiles.FirstOrDefault( item => item.Name == mFile.Name );

				if( listTrack != null ) {
					track.TrackNumber = (UInt16)( mFolderFiles.IndexOf( listTrack ) + 1 );
				}
			}
		}
	}
}
