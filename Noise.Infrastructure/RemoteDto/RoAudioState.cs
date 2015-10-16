using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoAudioState {
		[DataMember]
		public	int		VolumeLevel { get; set; }
	}
}
