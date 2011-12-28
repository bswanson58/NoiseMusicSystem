using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFolderProvider {
		void								AddFolder( StorageFolder folder );
		void								RemoveFolder( StorageFolder folder );

		StorageFolder						GetFolder( long folderId );
		string								GetPhysicalFolderPath( StorageFolder forFolder );

		DataProviderList<StorageFolder>		GetAllFolders();
		DataUpdateShell<StorageFolder>		GetFolderForUpdate( long folderId );
	}
}
