using System;
using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoServerInformation {
		[DataMember]
		public ServerVersion	ServerVersion { get; set; }
		[DataMember]
		public Int16			ApiVersion { get; set; }
		[DataMember]
		public string			ServerName { get; set; }
		[DataMember]
		public long				LibraryId { get; set; }
		[DataMember]
		public string			LibraryName { get; set; }
		[DataMember]
		public Int16			LibraryCount { get; set; }
		[DataMember]
		public RoAudioDevice[]	AudioDevices { get; set; }
		[DataMember]
		public int				CurrentAudioDevice { get; set; }

		public RoServerInformation() {
			ServerName = string.Empty;
			LibraryName = string.Empty;

			AudioDevices = new RoAudioDevice[0];
		}
	}
}
