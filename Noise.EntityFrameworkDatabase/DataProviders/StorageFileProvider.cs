using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class StorageFileProvider : BaseProvider<StorageFile>, IStorageFileProvider {
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IStorageFolderProvider	mStorageFolderProvider;

		public StorageFileProvider( IContextProvider contextProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IStorageFolderProvider folderProvider ) :
			base( contextProvider ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStorageFolderProvider = folderProvider;
		}

		public void AddFile( StorageFile file ) {
			AddItem( file );
		}

		public void DeleteFile( StorageFile file ) {
			RemoveItem( file );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			StorageFile	retValue;

			using( var context = CreateContext()) {
				retValue = Set( context ).FirstOrDefault( entity => entity.MetaDataPointer == forTrack.DbId );	
			}

			return( retValue );
		}

		public string GetPhysicalFilePath( StorageFile forFile ) {
			return( StorageHelpers.GetPath( mStorageFolderProvider, forFile ));
		}

		// This code - and FindCommonParent is duplicated in the Eloquera provider.
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

		public IDataProviderList<StorageFile> GetAllFiles() {
			return( GetListShell());
		}

		public IDataProviderList<StorageFile> GetDeletedFilesList() {
			var context = CreateContext();

			return( new EfProviderList<StorageFile>( context, Set( context ).Where( entity => entity.IsDeleted )));
		}

		public IDataProviderList<StorageFile> GetFilesInFolder( long parentFolder ) {
			var context = CreateContext();

			return( new EfProviderList<StorageFile>( context, Set( context ).Where( entity => entity.ParentFolder == parentFolder )));
		}

		public IDataProviderList<StorageFile> GetFilesOfType( eFileType fileType ) {
			var context = CreateContext();

			return( new EfProviderList<StorageFile>( context, Set( context ).Where( entity => entity.DbFileType == (int)fileType )));
		}

		public IDataUpdateShell<StorageFile> GetFileForUpdate( long fileId ) {
			return( GetUpdateShell( fileId ));
		}

		public long GetItemCount() {
			long	retValue;

			using( var context = CreateContext()) {
				retValue = Set( context ).Count();	
			}

			return( retValue );
		}

		private static string FindCommonParent( IEnumerable<string> paths ) {
			var retValue = "";
			var pathList = paths.Where( path => !string.IsNullOrWhiteSpace( path )).ToList();

			if( pathList.Any() ) {
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

	}
}
