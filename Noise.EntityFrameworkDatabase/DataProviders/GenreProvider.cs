using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class GenreProvider : BaseProvider<DbGenre>, IGenreProvider {
		public GenreProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		public void AddGenre( DbGenre genre ) {
			AddItem( genre );
		}

		public IDataProviderList<DbGenre> GetGenreList() {
			var context = CreateContext();

			return( new EfProviderList<DbGenre>( context, Set( context )));
		}
	}
}
