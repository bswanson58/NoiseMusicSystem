using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbAlbum : IUserSettings {
		public string			Name { get; set; }
		public long				Artist { get; set; }
		public Int16			Rating { get; set; }
		public Int16			TrackCount { get; set; }
		public UInt32			PublishedYear { get; set; }
		public string			Genre { get; set; }
		public bool				IsFavorite { get; set; }

		public DbAlbum() {
			Name = "";
			Genre = "";
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbAlbum )); }
		}
	}
}
