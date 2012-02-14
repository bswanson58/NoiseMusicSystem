using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class StorageFileProvider : BaseDataProvider<StorageFile>, IStorageFileProvider {
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IStorageFolderProvider	mStorageFolderProvider;

		public StorageFileProvider( IDatabaseManager databaseManager, IAlbumProvider albumProvider, ITrackProvider trackProvider, IStorageFolderProvider storageFolderProvider ) :
			base( databaseManager ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStorageFolderProvider = storageFolderProvider;
		}

		public void AddFile( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			InsertItem( file );
		}

		public void DeleteFile( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			DeleteItem( file );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			Condition.Requires( forTrack ).IsNotNull();

			return( TryGetItem( "SELECT StorageFile Where MetaDataPointer = @trackId", new Dictionary<string, object>{{ "trackId", forTrack.DbId }}, "GetPhysicalFile" ));
		}

		public string GetPhysicalFilePath( StorageFile forFile ) {
			return( StorageHelpers.GetPath( mStorageFolderProvider, forFile ));
		}

		public string GetAlbumPath( long albumId ) {
			var retValue = "";

			try {
				var album = mAlbumProvider.GetAlbum( albumId );

				if( album != null ) {
					using( var albumTracks = mTrackProvider.GetTrackList( album )) {
						var fileList = albumTracks.List.Select( GetPhysicalFile );
						var parentList = fileList.Select( file => file.ParentFolder ).Distinct();
						var folderList = parentList.Select( mStorageFolderProvider.GetFolder );
						var pathList = folderList.Select( folder => StorageHelpers.GetPath( mStorageFolderProvider, folder ));

						retValue = FindCommonParent( pathList );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetAlbumPath:", ex );
			}

			return( retValue );
		}

		public DataProviderList<StorageFile> GetAllFiles() {
			return( TryGetList( "SELECT StorageFile", "GetAllFiles" ));
		}

		public DataProviderList<StorageFile> GetDeletedFilesList() {
			return( TryGetList( "SELECT StorageFile WHERE IsDeleted", "GetDeletedFilesList" ));
		}

		public DataProviderList<StorageFile> GetFilesInFolder( long parentFolder ) {
			return( TryGetList( "SELECT StorageFile Where ParentFolder = @parentId", new Dictionary<string, object> {{ "parentId", parentFolder }}, "GetFilesInFolder" ));
		}

		public DataProviderList<StorageFile> GetFilesOfType( eFileType fileType ) {
			return( TryGetList( "SELECT StorageFile Where FileType = @fileType", new Dictionary<string, object> {{ "fileType", fileType }}, "GetFilesOfType" ));
		}

		public DataUpdateShell<StorageFile> GetFileForUpdate( long fileId ) {
			return( GetUpdateShell( "SELECT StorageFile Where DbId = @fileId", new Dictionary<string, object> {{ "fileId", fileId }}));
		}

		private static string FindCommonParent( IEnumerable<string> paths ) {
			var retValue = "";
			var pathList = paths.Where( path => !string.IsNullOrWhiteSpace( path )).ToList();

			if( pathList.Count() > 0 ) {
				if( pathList.Count() == 1 ) {
					retValue = pathList.First();
				}
				else {
					var match = true;
					var index = 0;

					retValue = pathList.First().Substring( 0, index + 1 );

					while(( match ) &&
						  ( index < pathList.First().Length )) {
						var matchString = retValue;

						match = pathList.All( path => path.StartsWith( matchString ));
						index++;

						if(( match ) &&
						   ( index < pathList.First().Length )) {
							retValue = pathList.First().Substring( 0, index + 1 );
						}
					}

					if(!match ) {
						var lastSlash = retValue.LastIndexOfAny( new [] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar } );

						if( lastSlash > 0 ) {
							retValue = retValue.Substring( 0, lastSlash + 1 );
						}
					}
				}
			}

			return( retValue );
		}

		public long GetItemCount() {
			return( GetItemCount( "SELECT StorageFile" ));
		}
	}
}
