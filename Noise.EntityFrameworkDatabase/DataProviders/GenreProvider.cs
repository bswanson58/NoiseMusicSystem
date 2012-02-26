using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class GenreProvider : BaseProvider<DbGenre>, IGenreProvider {
		public GenreProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddGenre( DbGenre genre ) {
			AddItem( genre );
		}

		public IDataProviderList<DbGenre> GetGenreList() {
			var context = CreateContext();

			return( new EfProviderList<DbGenre>( context, Set( context )));
		}
	}
}
