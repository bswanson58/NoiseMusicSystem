﻿using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class GenreProvider : BaseDataProvider<DbGenre>, IGenreProvider {
		public GenreProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddGenre( DbGenre genre ) {
			InsertItem( genre );
		}

		public IDataProviderList<DbGenre> GetGenreList() {
			return( TryGetList( "SELECT DbGenre", "Exception - GetGenreList" ));
		}
	}
}
