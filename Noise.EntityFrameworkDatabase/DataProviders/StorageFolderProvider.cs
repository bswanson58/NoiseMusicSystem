using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class StorageFolderProvider : BaseProvider<StorageFolder>, IStorageFolderProvider {
		public StorageFolderProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		public void AddFolder( StorageFolder folder ) {
			AddItem( folder );
		}

		public void RemoveFolder( StorageFolder folder ) {
			RemoveItem( folder );
		}

		public StorageFolder GetFolder( long folderId ) {
			return( GetItemByKey( folderId ));
		}

		public IDataProviderList<StorageFolder> GetAllFolders() {
			return( GetListShell());
		}

		public IDataProviderList<StorageFolder> GetChildFolders( long parentId ) {
			var context = CreateContext();

			return( new EfProviderList<StorageFolder>( context, Set( context ).Where( entity => entity.ParentFolder == parentId )));
		}

		public IDataProviderList<StorageFolder> GetDeletedFolderList() {
			var context = CreateContext();

			return( new EfProviderList<StorageFolder>( context, Set( context ).Where( entity => entity.IsDeleted )));
		}

		public IDataUpdateShell<StorageFolder> GetFolderForUpdate( long folderId ) {
			return( GetUpdateShell( folderId ));
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
