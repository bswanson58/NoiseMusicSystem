using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IArtworkProvider {
		void						AddArtwork( DbArtwork artwork );
		void						AddArtwork( DbArtwork artwork, byte[] pictureData );
		void						AddArtwork( DbArtwork artwork, string filePath );
		void						DeleteArtwork( DbArtwork artwork );

        Artwork                     GetArtwork( long artworkId );
		Artwork						GetArtistArtwork( long artistId, ContentType ofType );
		Artwork[]					GetAlbumArtwork( long albumId, ContentType ofType );
		Artwork[]					GetAlbumArtwork( long albumId );
	    DbArtwork[]                 GetAlbumArtworkInfo(long albumId, ContentType ofType);
	    DbArtwork[]                 GetAlbumArtworkInfo(long albumId);

        IDataProviderList<DbArtwork>	GetArtworkForFolder( long folderId );
		
		IDataUpdateShell<Artwork>	GetArtworkForUpdate( long artworkId );
		void						UpdateArtworkImage( long artworkId, string imageFilePath );
	}
}
