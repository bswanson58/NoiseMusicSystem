using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoSearchResultItem {
		[DataMember]
		public long		TrackId { get; set; }
		[DataMember]
		public string	TrackName { get; set; }
		[DataMember]
		public long		AlbumId { get; set; }
		[DataMember]
		public string	AlbumName { get; set; }
		[DataMember]
		public string	ArtistName { get; set; }
		[DataMember]
		public bool		CanPlay { get; set; }
	}
}
