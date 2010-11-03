using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	class SelectGenreDialogModel : DialogModelBase {
		private readonly List<DbGenre>	mGenreList;
		public	DbGenre					SelectedItem { get; set; }

		public SelectGenreDialogModel( IUnityContainer container ) {
			var noiseManager = container.Resolve<INoiseManager>();

			using( var genreList = noiseManager.DataProvider.GetGenreList()) {
				mGenreList = new List<DbGenre>( from DbGenre genre in genreList.List orderby genre.Name ascending  select genre );
			}
		}

		public IEnumerable<DbGenre> GenreList {
			get{ return( mGenreList ); }
		}
	}
}
