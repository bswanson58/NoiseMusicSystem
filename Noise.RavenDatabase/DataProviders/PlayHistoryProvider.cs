using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class PlayHistoryProvider : BaseProvider<DbPlayHistory>, IPlayHistoryProvider {
		public PlayHistoryProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddPlayHistory( DbPlayHistory playHistory ) {
			Database.Add( playHistory );
		}

		public void DeletePlayHistory( DbPlayHistory playHistory ) {
			Database.Delete( playHistory );
		}

		public IDataProviderList<DbPlayHistory> GetPlayHistoryList() {
			return( Database.FindAll());
		}

		public IDataUpdateShell<DbPlayHistory> GetPlayHistoryForUpdate( long playListId ) {
			return( new RavenDataUpdateShell<DbPlayHistory>( entity => Database.Update( entity ), Database.Get( playListId )));
		}
	}
}
