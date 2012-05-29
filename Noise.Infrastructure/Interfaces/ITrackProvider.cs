using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITrackProvider {
		void						AddTrack( DbTrack track );
		DbTrack						GetTrack( long trackId );
		void						DeleteTrack( DbTrack track );

		IDataProviderList<DbTrack>	GetTrackList( long albumId );
		IDataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		IDataProviderList<DbTrack>	GetTrackListForGenre( long genreId );
		IDataProviderList<DbTrack>	GetFavoriteTracks();
		IDataProviderList<DbTrack>	GetNewlyAddedTracks();
		IEnumerable<DbTrack>		GetTrackListForPlayList( DbPlayList playList );

		IDataUpdateShell<DbTrack>	GetTrackForUpdate( long trackId );

		long						GetItemCount();
	}
}
