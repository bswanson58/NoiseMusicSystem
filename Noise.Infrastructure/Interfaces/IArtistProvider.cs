using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IArtistProvider {
		void						AddArtist( DbArtist artist );
		void						DeleteArtist( DbArtist artist );

		DbArtist					GetArtist( long dbid );
		DbArtist					GetArtistForAlbum( DbAlbum album );
		DbArtist					FindArtist( string artistName );

		IDataProviderList<DbArtist>	GetArtistList();
		IDataProviderList<DbArtist>	GetChangedArtists( long changedSince );
		IDataProviderList<DbArtist>	GetFavoriteArtists();
		IDataUpdateShell<DbArtist>	GetArtistForUpdate( long artistId );
		void						UpdateArtistLastChanged( long artistId );
		IDataProviderList<long>		GetArtistCategories( long artistId );

		long						GetItemCount();
	}
}
