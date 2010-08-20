﻿using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbInternetStream : IUserSettings {
		[Required( AllowEmptyStrings=false, ErrorMessage = "Emty stream name is not allowed.")]
		public string			Name { get; set; }
		public string			Description { get; set; }
		[Required( AllowEmptyStrings=false, ErrorMessage = "Emty stream Url is not allowed.")]
		public string			Url { get; set; }
		public Int32			Bitrate { get; set; }
		public Int16			Channels { get; set; }
		public Int16			Rating { get; set; }
		public DateTime			DateAdded { get; private set; }
		public eAudioEncoding	Encoding { get; set; }
		public string			ExternalGenre { get; set; }
		public string			UserGenre { get; set; }
		public bool				IsPlaylistWrapped { get; set; }
		public bool				IsFavorite { get; set; }

		public DbInternetStream() {
			DateAdded = DateTime.Now;
			Encoding = eAudioEncoding.Unknown;
		}

		[Ignore]
		public string Genre {
			get{ return( String.IsNullOrWhiteSpace( UserGenre ) ?  ExternalGenre : UserGenre ); }
			set{ UserGenre = value; }
		}

		[Ignore]
		public bool IsUserRating {
			get{ return( true ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbInternetStream )); }
		}
	}
}
