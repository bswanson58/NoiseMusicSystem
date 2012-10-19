using System;
using System.Collections.Generic;

namespace Noise.Metadata.Dto {
	internal class DbArtistBiography {
		public	string					ArtistName { get; set; }
		public	List<StringMetadata>	Metadata { get; set; }

 		public DbArtistBiography() {
 			ArtistName = string.Empty;
			Metadata = new List<StringMetadata>();
 		}

		public void SetMetadata( eMetadataType metadataType, string metadata ) { }
		public void SetMetadata( eMetadataType metadataType, List<string> metadata ) { }
		public string GetMetadata( eMetadataType metadataType ) { return  string.Empty; }
		public List<String> GetMetadataArray( eMetadataType metadataType ) { return null; }
	}
}
