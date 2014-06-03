using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoAudioDevice {
		[DataMember]
		public	int			DeviceId { get; set; }
		[DataMember]
		public	string		Name {get; set; }

		public RoAudioDevice( AudioDevice audioDevice ) {
			DeviceId = audioDevice.DeviceId;
			Name = audioDevice.Name;
		}
	}
}
