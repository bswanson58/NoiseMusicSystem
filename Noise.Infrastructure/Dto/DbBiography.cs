using System;

namespace Noise.Infrastructure.Dto {
	public class DbBiography : ExpiringContent {
		public	long		Artist { get; private set; }
		public	DateTime	PublishedDate { get; set; }
		public	string		Biography { get; set; }

		public DbBiography( long artist ) {
			Artist = artist;
		}
	}
}
