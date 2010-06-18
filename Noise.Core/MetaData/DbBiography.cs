using System;

namespace Noise.Core.MetaData {
	public class DbBiography : ExpiringContent {
		public	long		Artist { get; private set; }
		public	DateTime	PublishedDate { get; set; }
		public	string		Biography { get; set; }

		public DbBiography( long artist ) {
			Artist = artist;
		}
	}
}
