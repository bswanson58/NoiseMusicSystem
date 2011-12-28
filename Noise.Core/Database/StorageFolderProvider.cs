using System.Collections.Generic;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class StorageFolderProvider : BaseDataProvider<StorageFolder>, IStorageFolderProvider {
		public StorageFolderProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddFolder( StorageFolder folder ) {
			using( var dbShell = CreateDatabase()) {
				dbShell.InsertItem( folder );
			}
		}

		public void RemoveFolder( StorageFolder folder ) {
			using( var dbShell = CreateDatabase()) {
				dbShell.DeleteItem( dbShell );
			}
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

		public DataUpdateShell<StorageFolder> GetFolderForUpdate( long folderId ) {
			return( GetUpdateShell( "SELECT StorageFolder Where DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }}));
		}
	}
}
