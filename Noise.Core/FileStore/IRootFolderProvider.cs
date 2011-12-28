using Noise.Infrastructure.Dto;

namespace Noise.Core.FileStore {
	internal interface IRootFolderProvider {
		void							AddRootFolder( RootFolder folder );

		DataProviderList<RootFolder>	GetRootFolderList();
		DataUpdateShell<RootFolder>	GetFolderForUpdate( long folderId );
	}
}
