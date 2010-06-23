using System;

namespace Noise.Infrastructure.Dto {
	public class DbTrack {
		public string			Name { get; set; }
		public long				Album { get; set; }
		public Int32			DurationMilliseconds { get; set; }
		public Int32			Bitrate { get; set; }
		public Int32			SampleRate { get; set; }
		public Int16			Channels { get; set; }
		public Int16			Rating { get; set; }
		public UInt16			TrackNumber { get; set; }
		public UInt32			PublishedYear { get; set; }
		public DateTime			DateAdded { get; private set; }
		public eAudioEncoding	Encoding { get; set; }
		public eMusicGenre		Genre { get; set; }

		public DbTrack() {
			DateAdded = DateTime.Now.Date;

			Album = Constants.cDatabaseNullOid;

			Encoding = eAudioEncoding.Unknown;
			Genre = eMusicGenre.Unknown;
		}

		public TimeSpan Duration {
			get{ return( new TimeSpan( 0, 0, 0, 0, DurationMilliseconds )); }
		}
	}
}
