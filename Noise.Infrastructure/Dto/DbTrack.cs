using System;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbTrack : IUserSettings {
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
		public string			CalculatedGenre { get; set; }
		public string			ExternalGenre { get; set; }
		public string			UserGenre { get; set; }
		public bool				IsFavorite { get; set; }

		public DbTrack() {
			Album = Constants.cDatabaseNullOid;
			DateAdded = DateTime.Now.Date;
			Encoding = eAudioEncoding.Unknown;

			CalculatedGenre = "";
			ExternalGenre = "";
			UserGenre = "";
			Performer = "";
			VolumeName = "";
		}

		[Ignore]
		public TimeSpan Duration {
			get{ return( new TimeSpan( 0, 0, 0, 0, DurationMilliseconds )); }
		}

		[Ignore]
		public string Genre {
			get{ return( String.IsNullOrWhiteSpace( UserGenre ) ? ( String.IsNullOrWhiteSpace( ExternalGenre ) ? CalculatedGenre : ExternalGenre ) : UserGenre ); }
			set{ UserGenre = value; }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTrack )); }
		}
	}
}
