using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class PlayQueueListResult : BaseResult {
		[DataMember]
		public RoPlayQueueTrack[]	Tracks { get; set; }
	}
}
