using System;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbAlbum : IUserSettings {
		public string			Name { get; set; }
		public long				Artist { get; set; }
		public Int16			Rating { get; set; }
		public Int16			TrackCount { get; set; }
		public UInt32			PublishedYear { get; set; }
		public string			CalculatedGenre { get; set; }
		public string			ExternalGenre { get; set; }
		public string			UserGenre { get; set; }
		public bool				IsFavorite { get; set; }

		public DbAlbum() {
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
			get{ return( typeof( DbAlbum )); }
		}
	}
}
