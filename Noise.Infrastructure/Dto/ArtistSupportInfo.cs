namespace Noise.Infrastructure.Dto {
	public class ArtistSupportInfo {
		public	DbTextInfo		Biography { get; private set; }
		public	DbArtwork		ArtistImage { get; private set; }
		public	DbSimilarItems	SimilarArtist { get; private set; }
		public	DbTopItems		TopAlbums { get; private set; }

		public ArtistSupportInfo( DbTextInfo biography, DbArtwork artistImage, DbSimilarItems similarArtists, DbTopItems topAlbums ) {
			Biography = biography;
			ArtistImage = artistImage;
			SimilarArtist = similarArtists;
			TopAlbums = topAlbums;
		}
	}
}
