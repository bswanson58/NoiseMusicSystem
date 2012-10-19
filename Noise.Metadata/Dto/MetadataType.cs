using System.Collections.Generic;

namespace Noise.Metadata.Dto {
	public enum eMetadataType {
		BandMembers,
		Biography,
		Genre,
		SimilarArtists,
		TopAlbums,
		WebSite,
		Unknown
	}

	public class StringMetadata {
		public	eMetadataType	MetadataType { get; set; }
		public	string				Metadata { get; set; }

		public StringMetadata() {
			MetadataType = eMetadataType.Unknown;
			Metadata = string.Empty;
		}

		public void FromArray( IEnumerable<string> array ) { }
 		public IEnumerable<string> ToArray() { return null; } 
	}
}
