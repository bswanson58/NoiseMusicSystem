using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProvider {
		long						DatabaseId { get; }

		DbArtist					GetArtist( long dbid );
		DataProviderList<DbArtist>	GetArtistList();
		DataProviderList<DbArtist>	GetArtistList( IDatabaseFilter filter );
		DbArtist					GetArtistForAlbum( DbAlbum album );
		DataProviderList<DbArtist>	GetFavoriteArtists();
		DataUpdateShell<DbArtist>	GetArtistForUpdate( long artistId );
		void						UpdateArtistLastChanged( long artistId );

		DbAlbum						GetAlbum( long dbid );
		DataProviderList<DbAlbum>	GetAlbumList( DbArtist forArtist );
		DataProviderList<DbAlbum>	GetAlbumList( long artistId );
		DbAlbum						GetAlbumForTrack( DbTrack track );
		DataProviderList<DbAlbum>	GetFavoriteAlbums();
		DataUpdateShell<DbAlbum>	GetAlbumForUpdate( long albumId );

		DbTrack						GetTrack( long trackId );
		DataProviderList<DbTrack>	GetTrackList( long albumId );
		DataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		DataProviderList<DbTrack>	GetFavoriteTracks();
		DataProviderList<DbTrack>	GetNewlyAddedTracks();
		DataUpdateShell<DbTrack>	GetTrackForUpdate( long trackId );

		DataProviderList<DbDiscographyRelease>	GetDiscography( long artistId );
		DataProviderList<DbPlayList>			GetPlayLists();

		DataProviderList<DbGenre>	GetGenreList();
		DataProviderList<DbTrack>	GetGenreTracks( long genreId );

		DataProviderList<DbTag>		GetTagList( eTagGroup forGroup );
		DataProviderList<DbTagAssociation>	GetTagAssociations( long forTagId );

		DataProviderList<long>		GetArtistCategories( long artistId );
		DataProviderList<long>		GetAlbumCategories( long albumId );
		void						SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories );

		StorageFile					GetPhysicalFile( DbTrack forTrack );
		string						GetPhysicalFilePath( StorageFile forFile );
		string						GetAlbumPath( long albumId );

		ArtistSupportInfo			GetArtistSupportInfo( long artistId );
		AlbumSupportInfo			GetAlbumSupportInfo( long albumId );

		DataUpdateShell<DbArtwork>	GetArtworkForUpdate( long artworkId );

		void						UpdateArtistInfo( long artistId );

		void						StoreLyric( DbLyric lyric );
		DataProviderList<DbLyric>	GetPossibleLyrics( DbArtist artist, DbTrack track );
		DataUpdateShell<DbLyric>	GetLyricForUpdate( long lyricId );

		void						InsertItem( object dbItem );
		void						DeleteItem( object dbItem );
		DbBase						GetItem( long itemId );

		DataProviderList<DbInternetStream>	GetStreamList();
		DbInternetStream			GetStream( long streamId );
		DataUpdateShell<DbInternetStream>	GetStreamForUpdate( long streamId );

		DataFindResults				Find( string artist, string album, string track );
		DataFindResults				Find( long itemId );

		long						GetTimestamp( string componentId );
		void						SetTimestamp( string componentId, long ticks );
	}
}
