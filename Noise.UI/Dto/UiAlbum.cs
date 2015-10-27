using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Album = {Name}")]
	public class UiAlbum : UiBase {
		public	long					Artist { get; set; }
		public	Int16					UserRating { get; set; }
		public	Int16					CalculatedRating { get; set; }
		public	Int16					TrackCount { get; set; }
		public	long					CalculatedGenre { get; set; }
		public	long					ExternalGenre { get; set; }
		public	long					UserGenre { get; set; }
		public	bool					IsFavorite { get; set; }
		public	bool					HasFavorites { get; set; }
		public	DbGenre					DisplayGenre { get ; set; }
		private readonly Action<long>	mOnPlay;

		public UiAlbum() { }

		public UiAlbum( Action<long> onPlay ) {
			mOnPlay = onPlay;
		}

		public string Name {
			get{ return( Get( () => Name )); }
			set{ Set( () => Name, value ); }
		}

		public string Genre {
			get{ return( DisplayGenre != null ? DisplayGenre.Name : "" ); }
		}

		public Int32 PublishedYear {
			get{ return( Get( () => PublishedYear )); }
			set{ Set( () => PublishedYear, value ); }
		}

		private bool IsUserRating {
			get{ return( UserRating != 0 ); }
		}

		public Int16 Rating {
			get{ return( IsUserRating ? UserRating : CalculatedRating ); }
			set{ UserRating = value; }
		}

		public override string ToString() {
			return( Name );
		}

		public void Execute_PlayAlbum() {
			if( mOnPlay != null ) {
				mOnPlay( DbId );
			}
		}
	}
}
