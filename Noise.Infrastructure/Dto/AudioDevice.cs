using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Device = {Name}")]
	public class AudioDevice {
		public	int			DeviceId { get; set; }
		public	bool		IsDefault { get ;set; }
		public	bool		IsEnabled { get; set; }
		public	string		Name { get; set; }

		public AudioDevice() {
			DeviceId = -1;
			Name = string.Empty;
		}
	}
}
