using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Genre = {Name}")]
	public class DbGenre : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			Description { get; set; }
		public Int16			Rating { get; set; }
		public bool				IsFavorite { get; set; }

		public DbGenre() {
			Name = string.Empty;
			Description = string.Empty;
		}

		public DbGenre( long id ) :
			base( id ) {
			Name = "";
			Description = "";
		}

		[Ignore]
		public long Genre {
			get{ return( DbId ); }
			set{ }
		}

		[Ignore]
		public bool IsUserRating {
			get { return( true ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbGenre )); }
		}
	}
}
