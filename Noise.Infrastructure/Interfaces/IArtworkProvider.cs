﻿using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IArtworkProvider {
		void						AddArtwork( DbArtwork artwork, byte[] pictureData );
		void						AddArtwork( DbArtwork artwork, string filePath );
		void						DeleteArtwork( DbArtwork artwork );

		Artwork						GetArtistArtwork( long artistId, ContentType ofType );
		Artwork[]					GetAlbumArtwork( long albumId, ContentType ofType );

		DataProviderList<DbArtwork>	GetArtworkForFolder( long folderId );

		DataUpdateShell<DbArtwork>	GetArtworkForUpdate( long artworkId );
	}
}
