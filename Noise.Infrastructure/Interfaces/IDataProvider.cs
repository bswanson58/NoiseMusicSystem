using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProvider {
		IEnumerable<DbArtist>	GetArtistList();
		IEnumerable<DbAlbum>	GetAlbumList( DbArtist forArtist );
		IEnumerable<DbTrack>	GetTrackList( DbAlbum forAlbum );
		IEnumerable<DbTrack>	GetTrackList( DbArtist forArtist );

		StorageFile				GetPhysicalFile( DbTrack forTrack );
		object					GetMetaData( StorageFile forFile );

		ArtistSupportInfo		GetArtistSupportInfo( DbArtist forArtist );
	}
}
