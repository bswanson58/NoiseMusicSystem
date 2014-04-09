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
		public string[]			TopTracks { get; set; }
		[DataMember]
		public long[]			TopTrackIds { get; set; }
		[DataMember]
		public string[]			SimilarArtists { get; set; }

		public RoArtistInfo() {
			Website = string.Empty;
			Biography = string.Empty;
			ArtistImage = string.Empty;
			BandMembers = new string[0];
			TopAlbums = new string[0];
			TopTracks = new string[0];
			TopTrackIds = new long[0];
			SimilarArtists = new string[0];
		}
	}
}
