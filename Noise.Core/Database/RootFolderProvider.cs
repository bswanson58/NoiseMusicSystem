using System.Collections.Generic;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class RootFolderProvider : BaseDataProvider<RootFolder>, IRootFolderProvider {
		public RootFolderProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddRootFolder( RootFolder folder ) {
			InsertItem( folder );
		}

		// Changes in the methods below are due to an Eloquera problem when returning an array of enums.

		public DataProviderList<RootFolder> GetRootFolderList() {
//			return( TryGetList( "SELECT RootFolder", "GetRootFolderList" ));

			DataProviderList<RootFolder>	retValue = null;

			using( var folderList = TryGetList( "SELECT RootFolder", "GetRootFolderList" )) {
				if(( folderList != null ) &&
				   ( folderList.List != null )) {
					var convertedList = new List<RootFolder>();

					foreach( var folder in folderList.List ) {
						folder.FolderStrategy.EloqueraFixUp();

						convertedList.Add( folder );
					}

					retValue = new DataProviderList<RootFolder>( null, convertedList );
				}
			}

			return( retValue );
		}

		public DataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
//			return( GetUpdateShell( "SELECT RootFolder Where DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }} ));

			var updater = GetUpdateShell( "SELECT RootFolder Where DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }} );

			if(( updater != null ) &&
			   ( updater.Item != null )) {
				updater.Item.FolderStrategy.EloqueraFixUp();
			}

			return( updater );
		}
	}
}
