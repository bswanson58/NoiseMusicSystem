using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Stream = {Name}")]
	public class DbInternetStream : DbBase, IUserSettings {
		[Required( AllowEmptyStrings=false, ErrorMessage = "Emty stream name is not allowed.")]
		public string			Name { get; set; }
		public string			Description { get; set; }
		[Required( AllowEmptyStrings=false, ErrorMessage = "Emty stream Url is not allowed.")]
		public string			Url { get; set; }
		public Int32			Bitrate { get; set; }
		public Int16			Channels { get; set; }
		public Int16			Rating { get; set; }
		public long				DateAddedTicks { get; protected set; }
		public eAudioEncoding	Encoding { get; set; }
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsPlaylistWrapped { get; set; }
		public bool				IsFavorite { get; set; }
		public string			Website { get; set; }

		public DbInternetStream() {
			DateAddedTicks = DateTime.Now.Date.Ticks;
			Encoding = eAudioEncoding.Unknown;

			Description = string.Empty;
			Website = string.Empty;

			ExternalGenre = Constants.cDatabaseNullOid;
			UserGenre = Constants.cDatabaseNullOid;
		}

		[Ignore]
		public long Genre {
			get{ return( UserGenre == Constants.cDatabaseNullOid ?  ExternalGenre : UserGenre ); }
			set{ UserGenre = value; }
		}

		[Ignore]
		public bool IsUserRating {
			get{ return( true ); }
		}

		[Ignore]
		public DateTime DateAdded {
			get{ return( new DateTime( DateAddedTicks )); }
		}

		[Ignore]
		public int DbAudioRecording {
			get{ return((int)Encoding ); }
			set{ Encoding = (eAudioEncoding)value; }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbInternetStream )); }
		}
	}
}
