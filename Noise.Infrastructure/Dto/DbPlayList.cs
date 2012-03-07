using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbPlayList : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			Description { get; set; }
		public List<long>		TrackIds { get; protected set; }
		public Int16			Rating { get; set; }
		public long				Genre { get; set; }
		public bool				IsFavorite { get; set; }

		public DbPlayList() {
			Name = string.Empty;
			Description = string.Empty;

			TrackIds = new List<long>();
		}

		public DbPlayList( string name, string description, IEnumerable<long> trackIds ) :
			this() {
			Name = name;
			Description = description;

			TrackIds.AddRange( trackIds );
		}

		[Ignore]
		public bool IsUserRating {
			get { return( true ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbPlayList )); }
		}
	}
}
