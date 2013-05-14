using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class PlayHistoryProvider : IPlayHistoryProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<DbPlayHistory>	mDatabase;

		public PlayHistoryProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbPlayHistory>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddPlayHistory( DbPlayHistory playHistory ) {
			mDatabase.Add( playHistory );
		}

		public void DeletePlayHistory( DbPlayHistory playHistory ) {
			mDatabase.Delete( playHistory );
		}

		public IDataProviderList<DbPlayHistory> GetPlayHistoryList() {
			return( new RavenDataProviderList<DbPlayHistory>( mDatabase.FindAll()));
		}

		public IDataUpdateShell<DbPlayHistory> GetPlayHistoryForUpdate( long playListId ) {
			return( new RavenDataUpdateShell<DbPlayHistory>( entity => mDatabase.Update( entity ), mDatabase.Get( playListId )));
		}
	}
}
