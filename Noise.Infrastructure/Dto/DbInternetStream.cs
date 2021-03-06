﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

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

		public long Genre {
			get{ return( UserGenre == Constants.cDatabaseNullOid ?  ExternalGenre : UserGenre ); }
			set{ UserGenre = value; }
		}

		public bool IsUserRating {
			get{ return( true ); }
		}

		public DateTime DateAdded {
			get{ return( new DateTime( DateAddedTicks )); }
		}
	}
}
