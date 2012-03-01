using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Album = {Name}")]
	public class UiAlbum : UiBase {
		public long				Artist { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			MaxChildRating { get; set; }
		public Int16			TrackCount { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }
		public DbGenre			DisplayGenre { get ; set; }

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
	}
}
