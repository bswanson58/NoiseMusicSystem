using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITrackProvider {
		void						AddTrack( DbTrack track );
		DbTrack						GetTrack( long trackId );

		DataProviderList<DbTrack>	GetTrackList( long albumId );
		DataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		DataProviderList<DbTrack>	GetTrackListForGenre( long genreId );
		DataProviderList<DbTrack>	GetFavoriteTracks();
		DataProviderList<DbTrack>	GetNewlyAddedTracks();
		IEnumerable<DbTrack>		GetTrackListForPlayList( DbPlayList playList );

		DataUpdateShell<DbTrack>	GetTrackForUpdate( long trackId );
	}
}
