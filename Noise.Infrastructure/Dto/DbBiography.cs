using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbBiography : ExpiringContent {
		public	long		Artist { get; private set; }
		public	DateTime	PublishedDate { get; set; }
		public	string		Biography { get; set; }
		public	byte[]		ArtistImage { get; set; }

		public DbBiography( long artist ) {
			Artist = artist;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbBiography )); }
		}
	}
}
