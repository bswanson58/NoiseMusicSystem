using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class AlbumListResult : BaseResult {
		[DataMember]
		public long			ArtistId { get; set; }
		[DataMember]
		public RoAlbum[]	Albums { get; set; }

		public AlbumListResult() {
			Albums = new RoAlbum[0];
		}
	}
}
