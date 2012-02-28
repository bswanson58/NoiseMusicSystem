using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class ArtworkProvider : BaseProvider<DbArtwork>, IArtworkProvider {
		public ArtworkProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		private Artwork TransformArtwork( DbArtwork artwork ) {
			return( new Artwork( artwork ) { Image = BlobStorage.RetrieveBytes( artwork.DbId ) });
		}

		public void AddArtwork( DbArtwork artwork ) {
			throw new System.NotImplementedException();
		}

		public void AddArtwork( DbArtwork artwork, byte[] pictureData ) {
			throw new System.NotImplementedException();
		}

		public void AddArtwork( DbArtwork artwork, string filePath ) {
			throw new System.NotImplementedException();
		}

		public void DeleteArtwork( DbArtwork artwork ) {
			throw new System.NotImplementedException();
		}

		public Artwork GetArtistArtwork( long artistId, ContentType ofType ) {
			throw new System.NotImplementedException();
		}

		public Artwork[] GetAlbumArtwork( long albumId, ContentType ofType ) {
			throw new System.NotImplementedException();
		}

		public Artwork[] GetAlbumArtwork( long albumId ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<DbArtwork> GetArtworkForFolder( long folderId ) {
			throw new System.NotImplementedException();
		}

		public IDataUpdateShell<Artwork> GetArtworkForUpdate( long artworkId ) {
			throw new System.NotImplementedException();
		}
	}
}
