using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class StorageFolderProvider : BaseProvider<StorageFolder>, IStorageFolderProvider {
		public StorageFolderProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddFolder( StorageFolder folder ) {
			Database.Add( folder );
		}

		public void RemoveFolder( StorageFolder folder ) {
			Database.Delete( folder );
		}

		public StorageFolder GetFolder( long folderId ) {
			return( Database.Get( folderId ));
		}

		public IDataProviderList<StorageFolder> GetAllFolders() {
			return( Database.FindAll());
		}

		public IDataProviderList<StorageFolder> GetChildFolders( long parentId ) {
			return( Database.Find( folder => folder.ParentFolder == parentId ));
		}

		public IDataProviderList<StorageFolder> GetDeletedFolderList() {
			return( Database.Find( folder => folder.IsDeleted ));
		}

		public IDataUpdateShell<StorageFolder> GetFolderForUpdate( long folderId ) {
			return( new RavenDataUpdateShell<StorageFolder>( folder => Database.Update( folder ), Database.Get( folderId )));
		}

		public long GetItemCount() {
			return( Database.Count());
		}
	}
}
