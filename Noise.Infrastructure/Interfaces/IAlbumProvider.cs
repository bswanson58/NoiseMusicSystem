using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAlbumProvider {
		void						AddAlbum( DbAlbum album );

		DbAlbum						GetAlbum( long dbid );
		DbAlbum						GetAlbumForTrack( DbTrack track );

		DataProviderList<DbAlbum>	GetAllAlbums();
		DataProviderList<DbAlbum>	GetAlbumList( DbArtist forArtist );
		DataProviderList<DbAlbum>	GetAlbumList( long artistId );
		DataProviderList<DbAlbum>	GetFavoriteAlbums();

		DataUpdateShell<DbAlbum>	GetAlbumForUpdate( long albumId );

		AlbumSupportInfo			GetAlbumSupportInfo( long albumId );

		DataProviderList<long>		GetAlbumsInCategory( long categoryId );
		DataProviderList<long>		GetAlbumCategories( long albumId );
		void						SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories );
	}
}
