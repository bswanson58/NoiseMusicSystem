using System;

namespace Noise.Infrastructure.RemoteDto {
	public class RoTrack : RoBase {
		public string			Name { get; set; }
		public Int32			DurationMilliseconds { get; set; }
		public Int16			Rating { get; set; }
		public UInt16			TrackNumber { get; set; }
		public string			VolumeName { get; set; }
		public bool				IsFavorite { get; set; }
	}
}
