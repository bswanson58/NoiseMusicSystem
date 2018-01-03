using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAlbumProvider {
		void						AddAlbum( DbAlbum album );
		void						DeleteAlbum( DbAlbum album );

		DbAlbum						GetAlbum( long dbid );
		DbAlbum						GetAlbumForTrack( DbTrack track );

		IDataProviderList<DbAlbum>	GetAllAlbums();
		IDataProviderList<DbAlbum>	GetAlbumList( DbArtist forArtist );
		IDataProviderList<DbAlbum>	GetAlbumList( long artistId );
		IDataProviderList<DbAlbum>	GetFavoriteAlbums();

		IDataUpdateShell<DbAlbum>	GetAlbumForUpdate( long albumId );

		AlbumSupportInfo			GetAlbumSupportInfo( long albumId );
        AlbumArtworkInfo            GetAlbumArtworkInfo( long albumId );

		IDataProviderList<long>		GetAlbumsInCategory( long categoryId );
		IDataProviderList<long>		GetAlbumCategories( long albumId );
		void						SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories );

		long						GetItemCount();
	}
}
