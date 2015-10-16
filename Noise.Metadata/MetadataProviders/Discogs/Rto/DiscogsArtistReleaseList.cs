using System.Collections.Generic;

namespace Noise.Metadata.MetadataProviders.Discogs.Rto {
	public class DiscogsArtistReleaseList {
		public	List<DiscogsRelease> Releases { get; set; }

		public DiscogsArtistReleaseList() {
			Releases = new List<DiscogsRelease>();
		}
	}
}
