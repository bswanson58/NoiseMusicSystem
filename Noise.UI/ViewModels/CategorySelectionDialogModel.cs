using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class CategorySelectionDialogModel : DialogModelBase {
		private readonly Func<object, IEnumerable<DbTag>>	mOnNewCategory;
		private readonly ObservableCollectionEx<UiCategory>	mCategoryList;

		public	List<long>		SelectedCategories { get; private set; }

		public CategorySelectionDialogModel( IEnumerable<DbTag> categoryList, IEnumerable<long> currentCategories, Func<object, IEnumerable<DbTag>> onAdd ) {
			mOnNewCategory = onAdd;
			mCategoryList = new ObservableCollectionEx<UiCategory>();
			SelectedCategories = new List<long>();

			mCategoryList.AddRange( from item in categoryList select TransformTag( item, currentCategories ));
		}

		private UiCategory TransformTag( DbTag tag, IEnumerable<long> selectedList ) {
			var retValue = new UiCategory( OnSelectionChanged );

			Mapper.DynamicMap( tag, retValue );
			retValue.IsSelected = selectedList.Contains( tag.DbId );

			return( retValue );
		}

		private void OnSelectionChanged( long tagId, bool selected ) {
			if( selected ) {
				SelectedCategories.Add( tagId );
			}
			else {
				SelectedCategories.Remove( tagId );
			}
		}

		public IEnumerable<UiCategory> CategoryList {
			get{ return( mCategoryList ); }
		}

		public void Execute_AddCategory() {
			if( mOnNewCategory != null ) {
				var	categoryList = mOnNewCategory( this );

				if( categoryList != null ) {
					mCategoryList.SuspendNotification();
					mCategoryList.Clear();
					mCategoryList.AddRange( from item in categoryList select TransformTag( item, SelectedCategories ));
					mCategoryList.ResumeNotification();
				}
			}
		}
	}
}
