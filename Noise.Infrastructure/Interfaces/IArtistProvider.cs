using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IArtistProvider {
		void						AddArtist( DbArtist artist );

		DbArtist					GetArtist( long dbid );
		DbArtist					GetArtistForAlbum( DbAlbum album );

		DataProviderList<DbArtist>	GetArtistList();
		DataProviderList<DbArtist>	GetArtistList( IDatabaseFilter filter );
		DataProviderList<DbArtist>	GetChangedArtists( long changedSince );
		DataProviderList<DbArtist>	GetFavoriteArtists();
		DataUpdateShell<DbArtist>	GetArtistForUpdate( long artistId );
		void						UpdateArtistLastChanged( long artistId );
		DataProviderList<long>		GetArtistCategories( long artistId );
		ArtistSupportInfo			GetArtistSupportInfo( long artistId );
	}
}
