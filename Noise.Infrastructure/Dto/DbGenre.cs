﻿using System;
using System.Diagnostics;

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

		public long Genre {
			get{ return( DbId ); }
			set{ }
		}

		public bool IsUserRating {
			get { return( true ); }
		}

		public override string ToString() {
			return( string.Format( "Genre \"{0}\"", Name ));
		}
	}
}
