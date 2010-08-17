using System;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbArtist : IUserSettings {
		public string			Name { get; set; }
		public string			CalculatedGenre { get; set; }
		public string			ExternalGenre { get; set; }
		public string			UserGenre { get; set; }
		public Int16			Rating { get; set; }
		public Int16			AlbumCount { get; set; }
		public bool				IsFavorite { get; set; }

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

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbArtist )); }
		}
	}
}
