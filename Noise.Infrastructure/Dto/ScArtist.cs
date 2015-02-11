using System;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("ScArtist = {ArtistName}")]
	public class ScArtist {
		public string	ArtistName { get; set; }
		public bool		IsFavorite { get; set; }
		public Int16	Rating { get; set; }
		public long		Version { get; set; }

		public ScArtist() {
			ArtistName = string.Empty;
		} 

		public ScArtist( DbArtist artist ) :
			this() {
			ArtistName = artist.Name;

			IsFavorite = artist.IsFavorite;
			Rating = artist.Rating;
			Version = artist.Version;
		}

		public void UpdateArtist( DbArtist artist ) {
			artist.IsFavorite = IsFavorite;
			artist.Rating = Rating;
			artist.Version = Version;
		}

		public override string ToString() {
			return( string.Format( "ScArtist \"{0}\", Version:{1}", ArtistName, Version ));
		}
	}
}
