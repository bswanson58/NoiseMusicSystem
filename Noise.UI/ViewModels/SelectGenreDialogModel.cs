using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	class SelectGenreDialogModel : DialogModelBase {
		private readonly List<DbGenre>	mGenreList;
		private	DbGenre					mSelectedItem;

		public IPlayStrategyParameters Parameters { get; private set; }

		public SelectGenreDialogModel( IGenreProvider genreProvider ) {
			using( var genreList = genreProvider.GetGenreList()) {
				mGenreList = new List<DbGenre>( from DbGenre genre in genreList.List orderby genre.Name ascending select genre );
			}
		}

		public IEnumerable<DbGenre> GenreList {
			get{ return( mGenreList ); }
		}

		public DbGenre SelectedItem {
			get { return ( mSelectedItem ); }
			set {
				mSelectedItem = value;

				if( mSelectedItem != null ) {
					Parameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayGenre ) { DbItemId = mSelectedItem.DbId };
				}
			}
		}
	}
}
