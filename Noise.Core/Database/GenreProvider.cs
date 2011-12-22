using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class GenreProvider : BaseDataProvider<DbGenre>, IGenreProvider {
		public GenreProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddGenre( DbGenre genre ) {
			InsertItem( genre );
		}

		public DataProviderList<DbGenre> GetGenreList() {
			return( TryGetList( "SELECT DbGenre", "Exception - GetGenreList" ));
		}
	}
}
