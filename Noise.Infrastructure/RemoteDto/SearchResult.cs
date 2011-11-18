using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class SearchResult : BaseResult {
		[DataMember]
		public RoSearchResultItem[]		Items { get; set; }
	}
}
