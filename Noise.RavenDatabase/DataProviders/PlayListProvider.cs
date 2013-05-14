using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class PlayListProvider : IPlayListProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<DbPlayList>	mDatabase;

		public PlayListProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbPlayList>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddPlayList( DbPlayList playList ) {
			mDatabase.Add( playList );
		}

		public void DeletePlayList( DbPlayList playList ) {
			mDatabase.Delete( playList );
		}

		public DbPlayList GetPlayList( long playListId ) {
			return( mDatabase.Get( playListId ));
		}

		public IDataProviderList<DbPlayList> GetPlayLists() {
			return( new RavenDataProviderList<DbPlayList>( mDatabase.FindAll()));
		}

		public IDataUpdateShell<DbPlayList> GetPlayListForUpdate( long playListId ) {
			return( new RavenDataUpdateShell<DbPlayList>( entity => mDatabase.Update( entity ), mDatabase.Get( playListId )));
		}
	}
}
