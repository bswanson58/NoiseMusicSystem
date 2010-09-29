using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProvider {
		long						GetObjectIdentifier( object dbObject );

		DbArtist					GetArtist( long dbid );
		DataProviderList<DbArtist>	GetArtistList();
		DataProviderList<DbArtist>	GetArtistList( IDatabaseFilter filter );
		DbArtist					GetArtistForAlbum( DbAlbum album );

		DbAlbum						GetAlbum( long dbid );
		DataProviderList<DbAlbum>	GetAlbumList( DbArtist forArtist );
		DbAlbum						GetAlbumForTrack( DbTrack track );

		DataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		List<DbTrack>				GetTrackList( DbArtist forArtist );

		StorageFile					GetPhysicalFile( DbTrack forTrack );

		ArtistSupportInfo			GetArtistSupportInfo( DbArtist forArtist );
		AlbumSupportInfo			GetAlbumSupportInfo( DbAlbum forAlbum );

		void						UpdateArtistInfo( DbArtist forArtist );
		void						UpdateAlbumInfo( DbAlbum forAlbum );

		void						SetFavorite( DbArtist forArtist, bool isFavorite );
		void						SetFavorite( DbAlbum forAlbum, bool isFavorite );
		void						SetFavorite( DbTrack forTrack, bool isFavorite );

		void						SetRating( DbArtist forArtist, Int16 rating );
		void						SetRating( DbAlbum forAlbum, Int16 rating );
		void						SetRating( DbTrack forTrack, Int16 rating );

		void						InsertItem( object dbItem );
		void						UpdateItem( object dbItem );
		void						DeleteItem( object dbItem );

		DataProviderList<DbInternetStream>	GetStreamList();
	}
}
