using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class ArtistInfoResult : BaseResult {
		[DataMember]
		public RoArtistInfo		ArtistInfo { get; set; }
	}
}
