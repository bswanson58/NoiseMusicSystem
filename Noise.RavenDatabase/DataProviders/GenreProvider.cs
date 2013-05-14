using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class GenreProvider : IGenreProvider {
		private readonly IDbFactory				mDbFactory;
		private readonly IRepository<DbGenre>	mDatabase;

		public GenreProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbGenre>( mDbFactory.GetLibraryDatabase(), genre => new object[] { genre.DbId });
		}

		public void AddGenre( DbGenre genre ) {
			mDatabase.Add( genre );
		}

		public IDataProviderList<DbGenre> GetGenreList() {
			return( new RavenDataProviderList<DbGenre>( mDatabase.FindAll()));
		}
	}
}
