using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Noise.Infrastructure.Dto {
	public class DbPlayList : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			Description { get; set; }
		public long[]			TrackIds { get; set; }
		public Int16			Rating { get; set; }
		public long				Genre { get; set; }
		public bool				IsFavorite { get; set; }

		public DbPlayList() {
			Name = "";
			Description = "";

			TrackIds = new long[0];
		}

		public DbPlayList( string name, string description, IEnumerable<long> trackIds ) {
			Name = name;
			Description = description;

			TrackIds = trackIds.ToArray();
		}

		public bool IsUserRating {
			get { return( true ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbPlayList )); }
		}
	}
}
