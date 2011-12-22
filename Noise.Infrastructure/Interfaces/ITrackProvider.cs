using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITrackProvider {
		DbTrack						GetTrack( long trackId );
		DataProviderList<DbTrack>	GetTrackList( long albumId );
		DataProviderList<DbTrack>	GetTrackList( DbAlbum forAlbum );
		DataProviderList<DbTrack>	GetFavoriteTracks();
		DataProviderList<DbTrack>	GetNewlyAddedTracks();
		DataUpdateShell<DbTrack>	GetTrackForUpdate( long trackId );
		DataProviderList<DbTrack>	GetGenreTracks( long genreId );
	}
}
