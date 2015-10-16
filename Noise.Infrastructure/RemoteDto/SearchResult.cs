using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class SearchResult : BaseResult {
		[DataMember]
		public RoSearchResultItem[]		Items { get; set; }
		[DataMember]
		public long[]					RandomTracks { get; set; }

		public SearchResult() {
			Items = new RoSearchResultItem[0];
			RandomTracks = new long[0];
		}
	}
}
