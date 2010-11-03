using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbInternetStream : DbBase, IUserSettings {
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
		public long				ExternalGenre { get; set; }
		public long				UserGenre { get; set; }
		public bool				IsPlaylistWrapped { get; set; }
		public bool				IsFavorite { get; set; }
		public string			Website { get; set; }

		public DbInternetStream() {
			DateAdded = DateTime.Now;
			Encoding = eAudioEncoding.Unknown;

			Description = "";
			Website = "";

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

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbInternetStream )); }
		}
	}
}
