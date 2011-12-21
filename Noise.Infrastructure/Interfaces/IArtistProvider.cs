using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IArtistProvider {
		DbArtist					GetArtist( long dbid );
		DataProviderList<DbArtist>	GetArtistList();
		DataProviderList<DbArtist>	GetArtistList( IDatabaseFilter filter );
		DbArtist					GetArtistForAlbum( DbAlbum album );
		DataProviderList<DbArtist>	GetFavoriteArtists();
		DataUpdateShell<DbArtist>	GetArtistForUpdate( long artistId );
		void						UpdateArtistLastChanged( long artistId );
		DataProviderList<long>		GetArtistCategories( long artistId );
		ArtistSupportInfo			GetArtistSupportInfo( long artistId );
	}
}
