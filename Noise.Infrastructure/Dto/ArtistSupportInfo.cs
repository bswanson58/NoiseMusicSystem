namespace Noise.Infrastructure.Dto {
	public class ArtistSupportInfo {
		public	DbTextInfo			Biography { get; private set; }
		public	DbArtwork			ArtistImage { get; private set; }
		public	DbAssociatedItems	SimilarArtist { get; private set; }
		public	DbAssociatedItems	TopAlbums { get; private set; }
		public	DbAssociatedItems	BandMembers { get; private set; }

		public ArtistSupportInfo( DbTextInfo biography, DbArtwork artistImage,
								  DbAssociatedItems similarArtists, DbAssociatedItems topAlbums, DbAssociatedItems bandMembers ) {
			Biography = biography;
			ArtistImage = artistImage;
			SimilarArtist = similarArtists;
			TopAlbums = topAlbums;
			BandMembers = bandMembers;
		}
	}
}
