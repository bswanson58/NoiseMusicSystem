using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class GenreProvider : BaseProvider<DbGenre>, IGenreProvider {
		public GenreProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddGenre( DbGenre genre ) {
			Database.Add( genre );
		}

		public IDataProviderList<DbGenre> GetGenreList() {
			return( Database.FindAll());
		}
	}
}
