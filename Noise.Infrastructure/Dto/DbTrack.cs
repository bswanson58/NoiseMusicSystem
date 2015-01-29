﻿using System;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Track = {Name}")]
	public class DbTrack : DbBase, IUserSettings {
		public string			Name { get; set; }
		public string			Performer { get; set; }
		public long				Artist { get; set; }
		public long				Album { get; set; }
		public Int32			DurationMilliseconds { get; set; }
		public Int32			Bitrate { get; set; }
		public Int32			SampleRate { get; set; }
		public Int16			BeatsPerMinute { get; set; }
		public Int16			Channels { get; set; }
		public Int16			Rating { get; set; }
		public Int16			TrackNumber { get; set; }
		public string			VolumeName { get; set; }
		public Int32			PublishedYear { get; set; }
		public long				DateAddedTicks { get; protected set; }
		public eAudioEncoding	Encoding { get; set; }
		public long				CalculatedGenre { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsFavorite { get; set; }
		public Int32			PlayCount { get; set; }
		public long				LastPlayedTicks { get; private set; }
		public float			ReplayGainAlbumGain { get; set; }
		public float			ReplayGainAlbumPeak { get; set; }
		public float			ReplayGainTrackGain { get; set; }
		public float			ReplayGainTrackPeak { get; set; }

		public DbTrack() {
			Album = Constants.cDatabaseNullOid;
			DateAddedTicks = DateTime.Now.Ticks;
			Encoding = eAudioEncoding.Unknown;

			CalculatedGenre = Constants.cDatabaseNullOid;
			ExternalGenre = Constants.cDatabaseNullOid;
			UserGenre = Constants.cDatabaseNullOid;
			Name = string.Empty;
			Performer = string.Empty;
			VolumeName = string.Empty;
		}

		public TimeSpan Duration {
			get{ return( new TimeSpan( 0, 0, 0, 0, DurationMilliseconds )); }
		}

		public long Genre {
			get{ return( UserGenre == Constants.cDatabaseNullOid ? ( ExternalGenre == Constants.cDatabaseNullOid ? CalculatedGenre : ExternalGenre ) : UserGenre ); }
			set{ UserGenre = value; }
		}

		public bool IsUserRating {
			get{ return( true ); }
		}

		public DateTime DateAdded {
			get{ return( new DateTime( DateAddedTicks )); }
		}

		public void UpdateLastPlayed() {
			PlayCount++;
			LastPlayedTicks = DateTime.Now.Ticks;
		}

		public override string ToString() {
			return( string.Format( "Track \"{0}\", Id:{1}, Artist:{2}, Album:{3}", Name, DbId, Artist, Album ));
		}
	}
}
