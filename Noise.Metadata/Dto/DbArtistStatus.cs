﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Noise.Metadata.Dto {
	[DebuggerDisplay("Provider = {Provider}")]
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

	[DebuggerDisplay("Artist = {ArtistName}")]
	internal class DbArtistStatus : IMetadataBase {
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

		public ProviderStatus GetProviderStatus( string forProvider ) {
			var retValue = ( from s in ProviderStatus where s.Provider == forProvider select s ).FirstOrDefault();

			if( retValue == null ) {
				retValue = new ProviderStatus { Provider = forProvider };

				retValue.LastUpdate -= ( retValue.Lifetime + retValue.Lifetime );
				ProviderStatus.Add( retValue );
			}

			return( retValue );
		}

		public void SetLastUpdate( string forProvider ) {
			var status = ( from s in ProviderStatus where s.Provider == forProvider select s ).FirstOrDefault();

			if( status == null ) {
				status = new ProviderStatus { Provider = forProvider };

				ProviderStatus.Add( status );
			}

			status.LastUpdate = DateTime.Now;
		}
	}
}
