using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class GenreProvider : BaseProvider<DbGenre>, IGenreProvider {
		public GenreProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
		}

		public void AddGenre( DbGenre genre ) {
			Database.Add( genre );
		}

		public IDataProviderList<DbGenre> GetGenreList() {
			return( Database.FindAll());
		}
	}
}
