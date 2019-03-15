using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	internal class NewTagRequest : Confirmation {
		public UiCategory	Category { get; private set; }

		public NewTagRequest( UiCategory category ) {
			Category = category;
			Content = category;
		}
	}

	public class CategorySelectionDialogModel : DialogModelBase {
		private readonly ITagProvider						mTagProvider;
		private readonly ObservableCollectionEx<UiCategory>	mCategoryList;
		private readonly InteractionRequest<NewTagRequest>	mNewTagRequest; 


		public	List<long>		SelectedCategories { get; private set; }

		public CategorySelectionDialogModel( ITagProvider tagProvider, IEnumerable<long> currentCategories ) {
			mTagProvider = tagProvider;
			mCategoryList = new ObservableCollectionEx<UiCategory>();
			SelectedCategories = new List<long>();

			using( var tagList = mTagProvider.GetTagList( eTagGroup.User )) {
				if(( tagList != null ) &&
				   ( tagList.List != null )) {
					mCategoryList.AddRange( from item in tagList.List select TransformTag( item, currentCategories ));
				}
			}

			mNewTagRequest = new InteractionRequest<NewTagRequest>();
		}

		private UiCategory TransformTag( DbTag tag, IEnumerable<long> selectedList ) {
			var retValue = new UiCategory( OnSelectionChanged );

			Mapper.Map( tag, retValue );
			retValue.IsSelected = selectedList.Contains( tag.DbId );

			return( retValue );
		}

		internal void OnSelectionChanged( long tagId, bool selected ) {
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

		public IInteractionRequest NewTagRequest {
			get{ return( mNewTagRequest ); }
		}

		public void Execute_AddCategory() {
			mNewTagRequest.Raise( new NewTagRequest( new UiCategory()), OnTagAdded );
		}

		private void OnTagAdded( NewTagRequest confirmation ) {
			if( confirmation.Confirmed ) {
				var tag = new DbTag( eTagGroup.User, confirmation.Category.Name ) { Description = confirmation.Category.Description };

				mTagProvider.AddTag( tag );
				mCategoryList.Add( TransformTag( tag, SelectedCategories ));
			}
		}
	}
}
