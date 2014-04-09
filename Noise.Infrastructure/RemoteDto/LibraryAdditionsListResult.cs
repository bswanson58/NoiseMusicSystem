using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class LibraryAdditionsListResult : BaseResult {
		[DataMember]
		public RoTrack[] NewTracks { get; set; }

		public LibraryAdditionsListResult() {
			NewTracks = new RoTrack[0];
		}
	}
}
