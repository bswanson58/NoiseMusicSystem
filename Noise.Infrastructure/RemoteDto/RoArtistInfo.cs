using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoArtistInfo {
		[DataMember]
		public long				ArtistId { get; set; }
		[DataMember]
		public string			Website { get; set; }
		[DataMember]
		public string			Biography { get; set; }
		[DataMember]
		public string			ArtistImage { get; set; }
		[DataMember]
		public string[]			BandMembers { get; set; }
		[DataMember]
		public string[]			TopAlbums { get; set; }
		[DataMember]
		public string[]			SimilarArtists { get; set; }
	}
}
