using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFolderProvider {
		void								AddFolder( StorageFolder folder );
		void								RemoveFolder( StorageFolder folder );

		StorageFolder						GetFolder( long folderId );
		string								GetPhysicalFolderPath( StorageFolder forFolder );

		DataProviderList<StorageFolder>		GetAllFolders();
		DataProviderList<StorageFolder>		GetChildFolders( long parentId );
		DataProviderList<StorageFolder>		GetDeletedFolderList();
		DataUpdateShell<StorageFolder>		GetFolderForUpdate( long folderId );
	}
}
