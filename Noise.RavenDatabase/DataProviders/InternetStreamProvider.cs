using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class InternetStreamProvider : IInternetStreamProvider {
		private readonly IDbFactory						mDbFactory;
		private readonly IRepository<DbInternetStream>	mDatabase;

		public InternetStreamProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbInternetStream>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddStream( DbInternetStream stream ) {
			mDatabase.Add( stream );
		}

		public void DeleteStream( DbInternetStream stream ) {
			mDatabase.Delete( stream );
		}

		public DbInternetStream GetStream( long streamId ) {
			return( mDatabase.Get( streamId ));
		}

		public IDataProviderList<DbInternetStream> GetStreamList() {
			return( new RavenDataProviderList<DbInternetStream>( mDatabase.FindAll()));
		}

		public IDataUpdateShell<DbInternetStream> GetStreamForUpdate( long streamId ) {
			return( new RavenDataUpdateShell<DbInternetStream>( stream => mDatabase.Update( stream ), mDatabase.Get( streamId )));
		}
	}
}
