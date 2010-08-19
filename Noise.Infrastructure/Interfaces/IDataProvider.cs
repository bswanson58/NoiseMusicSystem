using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProvider {
		long					GetObjectIdentifier( object dbObject );

		IEnumerable<DbArtist>	GetArtistList();
		DbArtist				GetArtistForAlbum( DbAlbum album );
		IEnumerable<DbAlbum>	GetAlbumList( DbArtist forArtist );
		DbAlbum					GetAlbumForTrack( DbTrack track );
		IEnumerable<DbTrack>	GetTrackList( DbAlbum forAlbum );
		IEnumerable<DbTrack>	GetTrackList( DbArtist forArtist );

		StorageFile				GetPhysicalFile( DbTrack forTrack );
		object					GetMetaData( StorageFile forFile );

		ArtistSupportInfo		GetArtistSupportInfo( DbArtist forArtist );
		AlbumSupportInfo		GetAlbumSupportInfo( DbAlbum forAlbum );

		void					UpdateArtistInfo( DbArtist forArtist );
		void					UpdateAlbumInfo( DbAlbum forAlbum );

		void					UpdateItem( object dbItem );
		void					DeleteItem( object dbItem );

		IEnumerable<DbInternetStream>	GetStreamList();
	}
}
