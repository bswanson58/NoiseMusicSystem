using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.ViewModels {
	class SelectCategoryDialogModel {
		private readonly List<DbTag>	mCategoryList;
		private	DbTag					mSelectedItem;

		public IPlayStrategyParameters Parameters { get; private set; }

		public SelectCategoryDialogModel( ITagProvider tagProvider ) {
			using( var genreList = tagProvider.GetTagList( eTagGroup.User )) {
				mCategoryList = new List<DbTag>( from DbTag tag in genreList.List orderby tag.Name ascending  select tag );
			}
		}

		public IEnumerable<DbTag> CategoryList {
			get{ return( mCategoryList ); }
		}

		public DbTag SelectedItem {
			get { return ( mSelectedItem ); }
			set {
				mSelectedItem = value;

				if( mSelectedItem != null ) {
					Parameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayCategory ) { DbItemId = mSelectedItem.DbId };
				}
			}
		}
	}
}
