using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class InternetStreamProvider : BaseProvider<DbInternetStream>, IInternetStreamProvider {
		public InternetStreamProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
		}

		public void AddStream( DbInternetStream stream ) {
			Database.Add( stream );
		}

		public void DeleteStream( DbInternetStream stream ) {
			Database.Delete( stream );
		}

		public DbInternetStream GetStream( long streamId ) {
			return( Database.Get( streamId ));
		}

		public IDataProviderList<DbInternetStream> GetStreamList() {
			return( Database.FindAll());
		}

		public IDataUpdateShell<DbInternetStream> GetStreamForUpdate( long streamId ) {
			return( new RavenDataUpdateShell<DbInternetStream>( stream => Database.Update( stream ), Database.Get( streamId )));
		}
	}
}
