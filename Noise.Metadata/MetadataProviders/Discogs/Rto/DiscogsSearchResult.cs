namespace Noise.Metadata.MetadataProviders.Discogs.Rto {
	public class DiscogsSearchResult {
		public ArtistSearchResult[]	Results { get; set; }
	}

	public class ArtistSearchResult {
		public string	Thumb { get; set; }
		public string	Title { get; set; }
		public string	Type { get; set; }
		public string	Id { get; set; }
	}
}
