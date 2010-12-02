using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Track = {Name}")]
	public class DbTrack : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			Performer { get; set; }
		public long				Album { get; set; }
		public Int32			DurationMilliseconds { get; set; }
		public Int32			Bitrate { get; set; }
		public Int32			SampleRate { get; set; }
		public Int16			Channels { get; set; }
		public Int16			Rating { get; set; }
		public UInt16			TrackNumber { get; set; }
		public string			VolumeName { get; set; }
		public UInt32			PublishedYear { get; set; }
		public DateTime			DateAdded { get; private set; }
		public eAudioEncoding	Encoding { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsFavorite { get; set; }
		public UInt32			PlayCount { get; set; }
		public float			ReplayGainAlbumGain { get; set; }
		public float			ReplayGainAlbumPeak { get; set; }
		public float			ReplayGainTrackGain { get; set; }
		public float			ReplayGainTrackPeak { get; set; }

		public DbTrack() {
			Album = Constants.cDatabaseNullOid;
			DateAdded = DateTime.Now.Date;
			Encoding = eAudioEncoding.Unknown;

			CalculatedGenre = Constants.cDatabaseNullOid;
			ExternalGenre = Constants.cDatabaseNullOid;
			UserGenre = Constants.cDatabaseNullOid;
			Performer = "";
			VolumeName = "";
		}

		[Ignore]
		public TimeSpan Duration {
			get{ return( new TimeSpan( 0, 0, 0, 0, DurationMilliseconds )); }
		}

		[Ignore]
		public long Genre {
			get{ return( UserGenre == Constants.cDatabaseNullOid ? ( ExternalGenre == Constants.cDatabaseNullOid ? CalculatedGenre : ExternalGenre ) : UserGenre ); }
			set{ UserGenre = value; }
		}

		[Ignore]
		public bool IsUserRating {
			get{ return( true ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTrack )); }
		}
	}
}
