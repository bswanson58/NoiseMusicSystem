namespace Noise.Infrastructure.Dto {
	public class ArtistSupportInfo  {
		public	DbTextInfo				Biography { get; private set; }
		public	DbArtwork				ArtistImage { get; private set; }
		public	DbAssociatedItemList	SimilarArtist { get; private set; }
		public	DbAssociatedItemList	TopAlbums { get; private set; }
		public	DbAssociatedItemList	BandMembers { get; private set; }

		public ArtistSupportInfo( DbTextInfo biography, DbArtwork artistImage,
								  DbAssociatedItemList similarArtists, DbAssociatedItemList topAlbums, DbAssociatedItemList bandMembers ) {
			Biography = biography;
			ArtistImage = artistImage;
			SimilarArtist = similarArtists;
			TopAlbums = topAlbums;
			BandMembers = bandMembers;
		}
	}
}
