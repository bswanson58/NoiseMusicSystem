using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class PlayListProvider : BaseProvider<DbPlayList>, IPlayListProvider {
		public PlayListProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddPlayList( DbPlayList playList ) {
			Database.Add( playList );
		}

		public void DeletePlayList( DbPlayList playList ) {
			Database.Delete( playList );
		}

		public DbPlayList GetPlayList( long playListId ) {
			return( Database.Get( playListId ));
		}

		public IDataProviderList<DbPlayList> GetPlayLists() {
			return( new RavenDataProviderList<DbPlayList>( Database.FindAll()));
		}

		public IDataUpdateShell<DbPlayList> GetPlayListForUpdate( long playListId ) {
			return( new RavenDataUpdateShell<DbPlayList>( entity => Database.Update( entity ), Database.Get( playListId )));
		}
	}
}
