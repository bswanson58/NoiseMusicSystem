using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProvider {
		long						DatabaseId { get; }

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

		DbTrack						GetTrack( long trackId );
		DataProviderList<DbTrack>	GetTrackList( long albumId );
		DataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		DataProviderList<DbTrack>	GetFavoriteTracks();
		DataProviderList<DbTrack>	GetNewlyAddedTracks();

		DataProviderList<DbDiscographyRelease>	GetDiscography( long artistId );
		DataProviderList<DbPlayList>			GetPlayLists();

		DataProviderList<DbGenre>	GetGenreList();
		DataProviderList<DbTrack>	GetGenreTracks( long genreId );

		StorageFile					GetPhysicalFile( DbTrack forTrack );
		string						GetPhysicalFilePath( StorageFile forFile );

		ArtistSupportInfo			GetArtistSupportInfo( long artistId );
		AlbumSupportInfo			GetAlbumSupportInfo( long albumId );

		void						UpdateArtistInfo( long artistId );

		void						InsertItem( object dbItem );
		void						DeleteItem( object dbItem );
		DbBase						GetItem( long itemId );

		DataProviderList<DbInternetStream>	GetStreamList();
		DbInternetStream			GetStream( long streamId );
		DataUpdateShell<DbInternetStream>	GetStreamForUpdate( long streamId );
	}
}
