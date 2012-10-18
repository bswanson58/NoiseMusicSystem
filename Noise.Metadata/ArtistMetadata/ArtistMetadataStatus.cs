using System;
using System.Collections.Generic;

namespace Noise.Metadata.ArtistMetadata {
	public class ProviderStatus {
		public	string		Provider { get; set; }
		public	DateTime	LastUpdate { get; set; }

		public ProviderStatus() {
			Provider = string.Empty;
			LastUpdate = DateTime.Now;
		}
	}

	public enum eArtistMetadataType {
		BandMembers,
		Biography,
		Genre,
		SimilarArtists,
		TopAlbums,
		WebSite,
		Unknown
	}

	public class StringMetadata {
		public	eArtistMetadataType	MetadataType { get; set; }
		public	string				Metadata { get; set; }

		public StringMetadata() {
			MetadataType = eArtistMetadataType.Unknown;
			Metadata = string.Empty;
		}

		public void FromArray( IEnumerable<string> array ) { }
 		public IEnumerable<string> ToArray() { return null; } 
	}

	public class ArtistMetadataInfo {
		public	string					ArtistName { get; set; }
		public	List<ProviderStatus>	ProviderUpdates { get; set; }
		public	List<StringMetadata>	Metadata { get; set; }

 		public ArtistMetadataInfo() {
 			ArtistName = string.Empty;
			ProviderUpdates = new List<ProviderStatus>();
			Metadata = new List<StringMetadata>();
 		}

		public void SetMetadata( eArtistMetadataType metadataType, string metadata ) { }
		public void SetMetadata( eArtistMetadataType metadataType, List<string> metadata ) { }
		public string GetMetadata( eArtistMetadataType metadataType ) { return  string.Empty; }
		public List<String> GetMetadataArray( eArtistMetadataType metadataType ) { return null; }

		public DateTime GetLastUpdate( string forProvider ) { return DateTime.Now; }
		public void SetLastUpdate( string forProvider ) { }
	}
}
