using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class PlayHistoryProvider : BaseProvider<DbPlayHistory>, IPlayHistoryProvider {
		public PlayHistoryProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
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
