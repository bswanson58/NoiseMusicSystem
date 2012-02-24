using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IRootFolderProvider {
		void							AddRootFolder( RootFolder folder );

		IDataProviderList<RootFolder>	GetRootFolderList();
		IDataUpdateShell<RootFolder>		GetFolderForUpdate( long folderId );
	}
}
