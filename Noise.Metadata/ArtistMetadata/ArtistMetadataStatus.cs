using System;
using System.Collections.Generic;

namespace Noise.Metadata.ArtistMetadata {
	internal class DateAssociation {
		public	string		Provider { get; set; }
		public	DateTime	LastUpdate { get; set; }
	}
	internal class ArtistMetadataStatus {
		public static string	Filename = "artistStatus";

		public	List<DateAssociation>	ProviderUpdates { get; set; }
	}
}
