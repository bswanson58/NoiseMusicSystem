using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class RootFolderProvider : IRootFolderProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<RootFolder>	mDatabase;

		public RootFolderProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<RootFolder>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddRootFolder( RootFolder folder ) {
			mDatabase.Add( folder );
		}

		public RootFolder GetRootFolder( long folderId ) {
			return( mDatabase.Get( folderId ));
		}

		public void DeleteRootFolder( RootFolder folder ) {
			mDatabase.Delete( folder );
		}

		public IDataProviderList<RootFolder> GetRootFolderList() {
			return( new RavenDataProviderList<RootFolder>( mDatabase.FindAll()));
		}

		public IDataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
			return( new RavenDataUpdateShell<RootFolder>( folder => mDatabase.Update( folder ), mDatabase.Get( folderId )));
		}
	}
}
