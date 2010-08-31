using System;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbArtist : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			CalculatedGenre { get; set; }
		public string			ExternalGenre { get; set; }
		public string			UserGenre { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			AlbumCount { get; set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }

		public DbArtist() {
			Name = "";
			CalculatedGenre = "";
			ExternalGenre = "";
			UserGenre = "";
		}

		[Ignore]
		public string Genre {
			get{ return( String.IsNullOrWhiteSpace( UserGenre ) ? ( String.IsNullOrWhiteSpace( ExternalGenre ) ? CalculatedGenre : ExternalGenre ) : UserGenre ); }
			set{ UserGenre = value; }
		}

		[Ignore]
		public Int16 Rating {
			get{ return( IsUserRating ? UserRating : CalculatedRating ); }
			set{ UserRating = value; }
		}

		[Ignore]
		public bool IsUserRating {
			get{ return( UserRating != 0 ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbArtist )); }
		}
	}
}
