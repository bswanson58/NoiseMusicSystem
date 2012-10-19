using System;
using System.Collections.Generic;

namespace Noise.Metadata.Dto {
	internal class ProviderStatus {
		public	string		Provider { get; set; }
		public	DateTime	LastUpdate { get; set; }
		public	TimeSpan	Lifetime { get; set; }

		public ProviderStatus() {
			Provider = string.Empty;
			LastUpdate = DateTime.Now;
			Lifetime = new TimeSpan( 30, 0, 0, 0 );
		}
	}

	internal class DbArtistStatus {
		private const string			cStatusKeyPrefix = "status/";

		public	string					ArtistName { get; set; }
		public	long					FirstMention { get; set; }
		public	List<ProviderStatus>	ProviderStatus { get; set; }

		public DbArtistStatus() {
			ArtistName = string.Empty;
			FirstMention = DateTime.Now.ToUniversalTime().Ticks;
			ProviderStatus = new List<ProviderStatus>();
		}

		public static string FormatStatusKey( string artistName ) {
			return( cStatusKeyPrefix + artistName.ToLower());
		}

		public string Id {
			get{ return( FormatStatusKey( ArtistName )); }
		}

		public DateTime GetLastUpdate( string forProvider ) { return DateTime.Now; }
		public void SetLastUpdate( string forProvider ) { }
	}
}
