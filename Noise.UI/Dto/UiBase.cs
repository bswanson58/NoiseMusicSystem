using System;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiBase : AutomaticPropertyBase {
		private Int16			mRating;
		private bool			mIsFavorite;

		public long				DbId { get; set; }

		public void SetRatings( bool isFavorite, Int16 rating ) {
			mIsFavorite = isFavorite;
			mRating = rating;
        }

		public Int16 UiRating {
			get => mRating;
			set {
				if( value != mRating ) {
					mRating = value;

					RaisePropertyChanged( () => UiRating );
                }
            }
		}

		public bool UiIsFavorite {
			get => mIsFavorite;
			set {
				if( value != mIsFavorite ) {
					mIsFavorite = value;

					RaisePropertyChanged( () => UiIsFavorite );
                }
            }
		}
	}
}
