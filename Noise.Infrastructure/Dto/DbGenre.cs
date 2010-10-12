using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbGenre : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			Description { get; set; }
		public Int16			Rating { get; set; }
		public bool				IsFavorite { get; set; }

		public DbGenre() {
			Name = "";
			Description = "";
		}

		public string Genre {
			get{ return( Name ); }
			set{ }
		}

		public bool IsUserRating {
			get { return( true ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbGenre )); }
		}
	}
}
