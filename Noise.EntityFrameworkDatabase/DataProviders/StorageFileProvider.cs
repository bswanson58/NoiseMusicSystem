using System.Collections.Generic;
using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class StorageFileProvider : BaseProvider<StorageFile>, IStorageFileProvider {
		public StorageFileProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddFile( StorageFile file ) {
			AddItem( file );
		}

		public void Add( IEnumerable<StorageFile> list ) {
			AddList( list );
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

		public StorageFile GetFileForMetadata( long metadataId ) {
			StorageFile	retValue;

			using( var context = CreateContext()) {
				retValue = Set( context ).FirstOrDefault( entity => entity.MetaDataPointer == metadataId );
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

		public IDataProviderList<StorageFile> GetFilesRequiringProcessing() {
			var context = CreateContext();

			return( new EfProviderList<StorageFile>( context, Set( context ).Where( entity => ( entity.FileType == eFileType.Undetermined ) || entity.WasUpdated )));
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
	}
}
