using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAlbumProvider {
		DbAlbum						GetAlbum( long dbid );
		DataProviderList<DbAlbum>	GetAlbumList( DbArtist forArtist );
		DataProviderList<DbAlbum>	GetAlbumList( long artistId );
		DbAlbum						GetAlbumForTrack( DbTrack track );
		DataProviderList<DbAlbum>	GetFavoriteAlbums();
		DataUpdateShell<DbAlbum>	GetAlbumForUpdate( long albumId );
		DataProviderList<long>		GetAlbumCategories( long albumId );
		void						SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories );
		AlbumSupportInfo			GetAlbumSupportInfo( long albumId );
	}
}
