using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class TrackProvider : BaseDataProvider<DbTrack>, ITrackProvider {
		public TrackProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public DbTrack GetTrack( long trackId ) {
			return( TryGetItem( "SELECT DbTrack Where DbId = @trackId", new Dictionary<string, object> {{ "trackId", trackId }}, "Exception - GetTrack" ));
		}

		public DataProviderList<DbTrack> GetTrackList( long albumId ) {
			return( TryGetList( "SELECT DbTrack WHERE Album = @albumId", new Dictionary<string, object>{{ "albumId", albumId }}, "Exception - GetTrackList" ));
		}

		public DataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			return( GetTrackList( forAlbum.DbId ));
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public DataProviderList<DbTrack> GetFavoriteTracks() {
			return( TryGetList( "SELECT DbTrack WHERE IsFavorite = true", "Exception - GetFavoriteTracks" ));
		}

		public DataProviderList<DbTrack> GetNewlyAddedTracks() {
			return( TryGetList( "SELECT DbTrack ORDER BY DateAddedTicks DESC", "Exception - GetNewlyAddedTracks" ));
		}

		public DataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			return( GetUpdateShell( "SELECT DbTrack Where DbId = @trackId", new Dictionary<string, object> {{ "trackId", trackId }} ));
		}

		public DataProviderList<DbTrack> GetTrackListForGenre( long genreId ) {
			return( TryGetList( "SELECT DbTrack Where Genre = @genre", new Dictionary<string, object> {{ "genre", genreId }}, "Exception - GetTrackListForGenre" ));
		}
	}
}
