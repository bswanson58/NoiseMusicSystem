using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFolderProvider {
		void								AddFolder( StorageFolder folder );
		void								RemoveFolder( StorageFolder folder );

		StorageFolder						GetFolder( long folderId );

		IDataProviderList<StorageFolder>	GetAllFolders();
		IDataProviderList<StorageFolder>	GetChildFolders( long parentId );
		IDataProviderList<StorageFolder>	GetDeletedFolderList();
		IDataUpdateShell<StorageFolder>		GetFolderForUpdate( long folderId );

		long								GetItemCount();
	}
}
