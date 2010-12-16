using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Album = {Name}")]
	public class DbAlbum : DbBase, IUserSettings {
		public string			Name { get; set; }
		public long				Artist { get; set; }
		public Int16			UserRating { get; set; }
		public Int16			CalculatedRating { get; set; }
		public Int16			MaxChildRating { get; set; }
		public Int16			TrackCount { get; set; }
		public UInt32			PublishedYear { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsFavorite { get; set; }
		public bool				HasFavorites { get; set; }
		public long				DateAddedTicks { get; private set; }

		public DbAlbum() {
			Name = "";
			CalculatedGenre = Constants.cDatabaseNullOid;
			ExternalGenre = Constants.cDatabaseNullOid;
			UserGenre = Constants.cDatabaseNullOid;
			DateAddedTicks = DateTime.Now.Date.Ticks;
		}

		[Ignore]
		public long Genre {
			get{ return( UserGenre == Constants.cDatabaseNullOid ? ExternalGenre == Constants.cDatabaseNullOid ? CalculatedGenre : ExternalGenre : UserGenre ); }
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

		[Ignore]
		public DateTime DateAdded {
			get{ return( new DateTime( DateAddedTicks )); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbAlbum )); }
		}
	}
}
