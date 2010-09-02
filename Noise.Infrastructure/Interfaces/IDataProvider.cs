using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProvider {
		long						GetObjectIdentifier( object dbObject );

		DataProviderList<DbArtist>	GetArtistList();
		DataProviderList<DbArtist>	GetArtistList( IDatabaseFilter filter );
		DbArtist					GetArtistForAlbum( DbAlbum album );
		DataProviderList<DbAlbum>	GetAlbumList( DbArtist forArtist );
		DbAlbum						GetAlbumForTrack( DbTrack track );
		DataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		List<DbTrack>				GetTrackList( DbArtist forArtist );

		StorageFile					GetPhysicalFile( DbTrack forTrack );

		ArtistSupportInfo			GetArtistSupportInfo( DbArtist forArtist );
		AlbumSupportInfo			GetAlbumSupportInfo( DbAlbum forAlbum );

		void						UpdateArtistInfo( DbArtist forArtist );
		void						UpdateAlbumInfo( DbAlbum forAlbum );

		void						UpdateItem( object dbItem );
		void						DeleteItem( object dbItem );

		DataProviderList<DbInternetStream>	GetStreamList();
	}
}
