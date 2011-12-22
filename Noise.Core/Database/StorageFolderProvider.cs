using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class StorageFolderProvider : BaseDataProvider<StorageFolder>, IStorageFolderProvider {
		public StorageFolderProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public StorageFolder GetFolder( long folderId ) {
			return( TryGetItem( "SELECT StorageFolder WHERE DbId = @folderId", new Dictionary<string, object> {{ "folderId", folderId }}, "GetStorageFolder" ));
		}
	}
}
