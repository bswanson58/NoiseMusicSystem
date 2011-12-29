using System.Collections.Generic;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class StorageFolderProvider : BaseDataProvider<StorageFolder>, IStorageFolderProvider {
		public StorageFolderProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddFolder( StorageFolder folder ) {
			InsertItem( folder );
		}

		public void RemoveFolder( StorageFolder folder ) {
			DeleteItem( folder );
		}

		public StorageFolder GetFolder( long folderId ) {
			return( TryGetItem( "SELECT StorageFolder WHERE DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }}, "GetStorageFolder" ));
		}

		public string GetPhysicalFolderPath( StorageFolder forFolder ) {
			return( StorageHelpers.GetPath( this, forFolder ));
		}

		public DataProviderList<StorageFolder> GetAllFolders() {
			return( TryGetList( "SELECT StorageFolder", "GetAllFolders" ));
		}

		public DataProviderList<StorageFolder> GetChildFolders( long parentId ) {
			return( TryGetList( "SELECT StorageFolder WHERE ParentFolder = @parentId", new Dictionary<string, object> {{ "parentId", parentId }}, "GetChildFolders" ));
		}

		public DataProviderList<StorageFolder> GetDeletedFolderList() {
			return( TryGetList( "SELECT StorageFolder WHERE IsDeleted", "GetDeletedFolderList" ));
		}

		public DataUpdateShell<StorageFolder> GetFolderForUpdate( long folderId ) {
			return( GetUpdateShell( "SELECT StorageFolder Where DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }}));
		}

		public long GetItemCount() {
			return( GetItemCount( "SELECT StorageFolder" ));
		}
	}
}
