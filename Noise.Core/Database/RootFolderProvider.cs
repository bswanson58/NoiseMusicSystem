using System.Collections.Generic;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class RootFolderProvider : BaseDataProvider<RootFolder>, IRootFolderProvider {
		public RootFolderProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddRootFolder( RootFolder folder ) {
			using( var dbShell = CreateDatabase()) {
				dbShell.InsertItem( folder );
			}
		}

		public DataProviderList<RootFolder> GetRootFolderList() {
			return( TryGetList( "SELECT RootFolder", "GetRootFolderList" ));
		}

		public DataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
			return( GetUpdateShell( "SELECT RootFolder Where DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }} ));
		}
	}
}
