using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.ViewModels {
	class SelectCategoryDialogModel {
		private readonly List<DbTag>	mCategoryList;
		public	DbTag					SelectedItem { get; set; }

		public SelectCategoryDialogModel( ITagProvider tagProvider ) {
			using( var genreList = tagProvider.GetTagList( eTagGroup.User )) {
				mCategoryList = new List<DbTag>( from DbTag tag in genreList.List orderby tag.Name ascending  select tag );
			}
		}

		public IEnumerable<DbTag> CategoryList {
			get{ return( mCategoryList ); }
		}
	}
}
