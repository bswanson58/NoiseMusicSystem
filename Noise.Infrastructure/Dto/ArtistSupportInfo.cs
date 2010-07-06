namespace Noise.Infrastructure.Dto {
	public class ArtistSupportInfo {
		public	DbBiography		Biography { get; private set; }
		public	DbSimilarItems	SimilarArtist { get; private set; }
		public	DbTopItems		TopAlbums { get; private set; }

		public ArtistSupportInfo( DbBiography biography, DbSimilarItems similarArtists, DbTopItems topAlbums ) {
			Biography = biography;
			SimilarArtist = similarArtists;
			TopAlbums = topAlbums;
		}
	}
}
