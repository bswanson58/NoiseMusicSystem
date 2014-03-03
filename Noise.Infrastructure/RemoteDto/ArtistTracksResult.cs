using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class ArtistTracksResult : BaseResult {
		[DataMember]
		public long		ArtistId { get; set; }
		[DataMember]
		public long		AlbumCount { get; set; }
		[DataMember]
		public long		UniqueTrackCount { get; set; }
	}
}
