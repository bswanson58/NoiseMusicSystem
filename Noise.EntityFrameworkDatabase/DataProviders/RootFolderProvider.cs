using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class RootFolderProvider : BaseProvider<RootFolder>, IRootFolderProvider {
		public RootFolderProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddRootFolder( RootFolder folder ) {
			AddItem( folder );
		}

		public IDataProviderList<RootFolder> GetRootFolderList() {
			return( GetListShell());
		}

		public IDataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
			return( GetUpdateShell( folderId ));
		}
	}
}
