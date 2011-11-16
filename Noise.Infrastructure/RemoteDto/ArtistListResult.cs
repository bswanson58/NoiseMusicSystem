using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class ArtistListResult : BaseResult {
		[DataMember]
		public	RoArtist[]		Artists { get; set; }

		public ArtistListResult() {
			Artists = new RoArtist[0];
		}
	}
}
