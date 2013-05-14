using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class StorageFolderProvider : IStorageFolderProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<StorageFolder>	mDatabase;

		public StorageFolderProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<StorageFolder>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddFolder( StorageFolder folder ) {
			mDatabase.Add( folder );
		}

		public void RemoveFolder( StorageFolder folder ) {
			mDatabase.Delete( folder );
		}

		public StorageFolder GetFolder( long folderId ) {
			return( mDatabase.Get( folderId ));
		}

		public IDataProviderList<StorageFolder> GetAllFolders() {
			return( new RavenDataProviderList<StorageFolder>( mDatabase.FindAll()));
		}

		public IDataProviderList<StorageFolder> GetChildFolders( long parentId ) {
			return( new RavenDataProviderList<StorageFolder>( mDatabase.Find( folder => folder.ParentFolder == parentId )));
		}

		public IDataProviderList<StorageFolder> GetDeletedFolderList() {
			return( new RavenDataProviderList<StorageFolder>( mDatabase.Find( folder => folder.IsDeleted )));
		}

		public IDataUpdateShell<StorageFolder> GetFolderForUpdate( long folderId ) {
			return( new RavenDataUpdateShell<StorageFolder>( folder => mDatabase.Update( folder ), mDatabase.Get( folderId )));
		}

		public long GetItemCount() {
			return( mDatabase.Count());
		}
	}
}
