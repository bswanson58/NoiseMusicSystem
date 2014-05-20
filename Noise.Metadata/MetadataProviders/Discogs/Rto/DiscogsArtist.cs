namespace Noise.Metadata.MetadataProviders.Discogs.Rto {
	public class DiscogsArtist {
		public	string				Id { get; set; }
		public	string				Name { get; set; }
		public	string				RealName { get; set; }
		public	string				Profile { get; set; }
		public	string[]			Urls {  get; set; }
		public	string[]			NameVariations { get; set; }
		public	DiscogsBandMember[] Members { get; set; }
		public	DiscogsBandMember[] Groups { get; set; }
		public	DiscogsImage[]		Images { get; set; }
		public	string				Releases_Url { get; set; }
		public string				Data_Quality { get; set; }
	}
}
