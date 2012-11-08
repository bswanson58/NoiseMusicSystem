namespace Noise.Metadata.Dto {
	internal class DbArtistDiscography : IMetadataBase {
		private const string	cStatusKeyPrefix = "disco/";

		public	string			ArtistName { get; set; }

		public static string FormatStatusKey( string artistName ) {
			return( cStatusKeyPrefix + artistName.ToLower());
		}

		public string Id {
			get{ return( FormatStatusKey( ArtistName )); }
		}
	}
}
