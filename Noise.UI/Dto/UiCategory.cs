using System;
using System.Diagnostics;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Category = {Name}")]
	public class UiCategory : UiBase {
		private readonly Action<long, bool>	mOnSelection;
		private bool						mIsSelected;

		public string		Name { get; set; }
		public string		Description { get; set; }

		public UiCategory() { }

		public UiCategory( Action<long, bool> onSelected ) {
			mOnSelection = onSelected;
		}

		public bool IsSelected {
			get{ return( mIsSelected ); }
			set {
				if( mIsSelected != value ) {
					mIsSelected = value;

					if( mOnSelection != null ) {
						mOnSelection( DbId, mIsSelected );
					}
				}
			}
		}
	}
}
