using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class PlayListProvider : BaseProvider<DbPlayList>, IPlayListProvider {
		public PlayListProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
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
			return( Database.FindAll());
		}

		public IDataUpdateShell<DbPlayList> GetPlayListForUpdate( long playListId ) {
			return( new RavenDataUpdateShell<DbPlayList>( entity => Database.Update( entity ), Database.Get( playListId )));
		}
	}
}
