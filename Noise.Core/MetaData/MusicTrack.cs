using System;

namespace Noise.Core.MetaData {
	public class MusicTrack {
		public string			Name { get; set; }
		public TimeSpan			Length { get; set; }
		public Int32			Bitrate { get; set; }
		public Int16			Rating { get; set; }
		public UInt16			TrackNumber { get; set; }
		public UInt16			PublishedYear { get; set; }
		public DateTime			DateAdded { get; private set; }
		public eAudioEncoding	Encoding { get; set; }
		public eMusicGenre		Genre { get; set; }

		public MusicTrack() {
			DateAdded = DateTime.Now.Date;

			Encoding = eAudioEncoding.Unknown;
			Genre = eMusicGenre.Unknown;
		}
	}
}
