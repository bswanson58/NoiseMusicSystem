using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class AudioStateResult : BaseResult {
		[DataMember]
		public RoAudioState AudioState { get; set; }

		public AudioStateResult() {
			AudioState = new RoAudioState();
		}
	}
}
