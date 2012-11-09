using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Metadata.Dto {
	internal class DbArtistDiscography : IMetadataBase, IArtistDiscography {
		private const string	cStatusKeyPrefix = "disco/";

		public	string					ArtistName { get; set; }
		public	List<DbDiscographyRelease>	Discography { get; set; }

		public DbArtistDiscography() {
			ArtistName = string.Empty;
			Discography = new List<DbDiscographyRelease>();
		}

		public static string FormatStatusKey( string artistName ) {
			return( cStatusKeyPrefix + artistName.ToLower());
		}

		public string Id {
			get{ return( FormatStatusKey( ArtistName )); }
		}
	}
}
