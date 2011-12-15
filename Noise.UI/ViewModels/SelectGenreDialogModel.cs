using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	class SelectGenreDialogModel : DialogModelBase {
		private readonly List<DbGenre>	mGenreList;
		public	DbGenre					SelectedItem { get; set; }

		public SelectGenreDialogModel( IDataProvider dataProvider ) {
			using( var genreList = dataProvider.GetGenreList()) {
				mGenreList = new List<DbGenre>( from DbGenre genre in genreList.List orderby genre.Name ascending  select genre );
			}
		}

		public IEnumerable<DbGenre> GenreList {
			get{ return( mGenreList ); }
		}
	}
}
