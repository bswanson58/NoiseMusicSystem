using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class RootFolderProvider : BaseDataProvider<RootFolder>, IRootFolderProvider {
		public RootFolderProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddRootFolder( RootFolder folder ) {
			InsertItem( folder );
		}

		public RootFolder GetRootFolder( long folderId ) {
			return( TryGetItem( "SELECT RootFolder WHERE DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }}, "GetRootFolder" ));
		}

		public void DeleteRootFolder( RootFolder folder ) {
			Condition.Requires( folder ).IsNotNull();

			DeleteItem( folder );
		}

		// Changes in the methods below are due to an Eloquera problem when returning an array of enums.

		public IDataProviderList<RootFolder> GetRootFolderList() {
			return( TryGetList( "SELECT RootFolder", "GetRootFolderList" ));
		}

		public IDataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
			return( GetUpdateShell( "SELECT RootFolder Where DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }} ));
		}
	}
}
