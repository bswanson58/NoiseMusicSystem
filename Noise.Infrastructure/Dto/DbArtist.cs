using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbArtist {
		public string			Name { get; set; }
		public string			Genre { get; set; }
		public Int16			Rating { get; set; }
		public Int16			AlbumCount { get; set; }
		public bool				IsFavorite { get; set; }

		public DbArtist() {
			Name = "";
			Genre = "";
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbArtist )); }
		}
	}
}
