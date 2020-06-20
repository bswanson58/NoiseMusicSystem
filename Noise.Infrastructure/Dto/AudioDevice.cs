using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Device = {" + nameof( Name ) + "}")]
	public class AudioDevice {
		public	int			DeviceId { get; set; }
		public	bool		IsDefault { get ;set; }
		public	bool		IsEnabled { get; set; }
		public	bool		WillMakeNoise {  get; set; }
		public	string		Name { get; set; }

		public AudioDevice() {
			DeviceId = -1;
			Name = string.Empty;
		}

		public override string ToString() {
			return( $"Audio device \"{Name}\"" );
		}
	}
}
