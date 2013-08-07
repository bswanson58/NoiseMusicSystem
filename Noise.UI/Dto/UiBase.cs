using System;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiBase : AutomaticCommandBase {
		public long				DbId { get; set; }

		public Int16 UiRating {
			get{ return( Get( () => UiRating )); }
			set{ Set( () => UiRating, value ); }
		}

		public bool UiIsFavorite {
			get{ return( Get( () => UiIsFavorite )); }
			set{ Set( () => UiIsFavorite, value ); }
		}
	}
}
