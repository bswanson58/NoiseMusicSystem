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
		DataProviderList<DbArtist>	GetFavoriteArtists();

		DbAlbum						GetAlbum( long dbid );
		DataProviderList<DbAlbum>	GetAlbumList( DbArtist forArtist );
		DataProviderList<DbAlbum>	GetAlbumList( long artistId );
		DbAlbum						GetAlbumForTrack( DbTrack track );
		DataProviderList<DbAlbum>	GetFavoriteAlbums();

		DbTrack						GetTrack( StorageFile forFile );
		DbTrack						GetTrack( long trackId );
		DataProviderList<DbTrack>	GetTrackList( long albumId );
		DataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		List<DbTrack>				GetTrackList( DbArtist forArtist );
		DataProviderList<DbTrack>	GetFavoriteTracks();
		DataProviderList<DbTrack>	GetNewlyAddedTracks();

		DataProviderList<DbDiscographyRelease>	GetDiscography( long artistId );
		DataProviderList<DbDiscographyRelease>	GetDiscography( DbArtist forArtist );
		DataProviderList<DbPlayList>			GetPlayLists();

		DataProviderList<DbGenre>	GetGenreList();

		StorageFile					GetPhysicalFile( DbTrack forTrack );

		ArtistSupportInfo			GetArtistSupportInfo( long artistId );
		ArtistSupportInfo			GetArtistSupportInfo( DbArtist forArtist );
		AlbumSupportInfo			GetAlbumSupportInfo( long albumId );
		AlbumSupportInfo			GetAlbumSupportInfo( DbAlbum forAlbum );

		void						UpdateArtistInfo( DbArtist forArtist );
		void						UpdateArtistInfo( long artistId );
		void						UpdateAlbumInfo( DbAlbum forAlbum );

		void						SetFavorite( DbArtist forArtist, bool isFavorite );
		void						SetArtistFavorite( long artistId, bool isFavorite );
		void						SetFavorite( DbAlbum forAlbum, bool isFavorite );
		void						SetAlbumFavorite( long albumId, bool isFavorite );
		void						SetFavorite( DbTrack forTrack, bool isFavorite );
		void						SetTrackFavorite( long trackId, bool isFavorite );
		void						SetFavorite( DbPlayList forList, bool isFavorite );

		void						SetRating( DbArtist forArtist, Int16 rating );
		void						SetArtistRating( long artistId, Int16 rating );
		void						SetRating( DbAlbum forAlbum, Int16 rating );
		void						SetAlbumRating( long albumId, Int16 rating );
		void						SetRating( DbTrack forTrack, Int16 rating );
		void						SetTrackRating( long trackId, Int16 rating );
		void						SetRating( DbPlayList forList, Int16 rating );

		void						InsertItem( object dbItem );
		void						UpdateItem( object dbItem );
		void						DeleteItem( object dbItem );

		DataProviderList<DbInternetStream>	GetStreamList();
		DbInternetStream			GetStream( long streamId );
	}
}
