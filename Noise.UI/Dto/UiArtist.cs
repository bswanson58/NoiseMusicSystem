using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Artist = {Name}")]
	public class UiArtist : UiBase {
		public string			SortName { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			MaxChildRating { get; set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }

		public string ActiveYears {
			get{ return( Get( () => ActiveYears )); }
			set{ Set( () => ActiveYears, value ); }
		}

		public Int16 AlbumCount {
			get{ return( Get( () => AlbumCount )); }
			set{ Set( () => AlbumCount, value ); }
		}

		public DbGenre DisplayGenre {
			get{ return( Get( () => DisplayGenre )); }
			set{ Set( () => DisplayGenre, value ); }
		}

		[DependsUpon("DisplayGenre")]
		public string Genre {
			get{ return( DisplayGenre != null ? DisplayGenre.Name : "" ); }
		}

		private bool IsUserRating {
			get{ return( UserRating != 0 ); }
		}

		public string Name {
			get{ return( Get( () => Name )); }
			set{ Set( () => Name, value ); }
		}

		public string DisplayName {
			get{ return( Get( () => DisplayName )); }
			set{ Set( () => DisplayName, value ); }
		}

		public Int16 Rating {
			get{ return( IsUserRating ? UserRating : CalculatedRating ); }
			set{ UserRating = value; }
		}

		public string Website {
			get{ return( Get( () => Website )); }
			set{ Set( () => Website, value ); }
		}

		public override string ToString() {
			return( Name );
		}
	}
}
