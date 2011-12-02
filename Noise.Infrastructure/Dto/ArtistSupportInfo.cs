namespace Noise.Infrastructure.Dto {
	public class ArtistSupportInfo  {
		public	TextInfo				Biography { get; private set; }
		public	Artwork					ArtistImage { get; private set; }
		public	DbAssociatedItemList	SimilarArtist { get; private set; }
		public	DbAssociatedItemList	TopAlbums { get; private set; }
		public	DbAssociatedItemList	BandMembers { get; private set; }

		public ArtistSupportInfo( TextInfo biography, Artwork artistImage,
								  DbAssociatedItemList similarArtists, DbAssociatedItemList topAlbums, DbAssociatedItemList bandMembers ) {
			Biography = biography;
			ArtistImage = artistImage;
			SimilarArtist = similarArtists;
			TopAlbums = topAlbums;
			BandMembers = bandMembers;
		}
	}
}
