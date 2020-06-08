using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Category = {" + nameof( Name ) + "}")]
	public class UiCategory : UiBase {
		private readonly Action<long, bool>	mOnSelection;
		private bool						mIsSelected;

		public string		Name { get; set; }
		public string		Description { get; set; }

		public UiCategory() { }

		public UiCategory( DbTag tag, Action<long, bool> onSelected ) {
			mOnSelection = onSelected;

			if( tag != null ) {
                DbId = tag.DbId;
                Name = tag.Name;
                Description = tag.Description;
                UiIsFavorite = tag.IsFavorite;
                UiRating = tag.Rating;
            }
		}

		public bool IsSelected {
			get => ( mIsSelected );
            set {
				if( mIsSelected != value ) {
					mIsSelected = value;

                    mOnSelection?.Invoke( DbId, mIsSelected );
                }
			}
		}
	}
}
