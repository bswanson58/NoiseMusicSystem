using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class TrackProvider : BaseDataProvider<DbTrack>, ITrackProvider {
		public TrackProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddTrack( DbTrack track ) {
			Condition.Requires( track ).IsNotNull();

			InsertItem( track );
		}

		public void DeleteTrack( DbTrack track ) {
			DeleteItem( track );
		}

		public DbTrack GetTrack( long trackId ) {
			return( TryGetItem( "SELECT DbTrack Where DbId = @trackId", new Dictionary<string, object> {{ "trackId", trackId }}, "GetTrack" ));
		}

		public IDataProviderList<DbTrack> GetTrackList( long albumId ) {
			return( TryGetList( "SELECT DbTrack WHERE Album = @albumId", new Dictionary<string, object>{{ "albumId", albumId }}, "GetTrackList" ));
		}

		public IDataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			return( GetTrackList( forAlbum.DbId ));
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public IDataProviderList<DbTrack> GetFavoriteTracks() {
			return( TryGetList( "SELECT DbTrack WHERE IsFavorite = true", "GetFavoriteTracks" ));
		}

		public IDataProviderList<DbTrack> GetNewlyAddedTracks() {
			return( TryGetList( "SELECT DbTrack ORDER BY DateAddedTicks DESC", "GetNewlyAddedTracks" ));
		}

		public IDataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			return( GetUpdateShell( "SELECT DbTrack Where DbId = @trackId", new Dictionary<string, object> {{ "trackId", trackId }} ));
		}

		public IDataProviderList<DbTrack> GetTrackListForGenre( long genreId ) {
			return( TryGetList( "SELECT DbTrack Where UserGenre = @genre", new Dictionary<string, object> {{ "genre", genreId }}, "GetTrackListForGenre" ));
		}

		public long GetItemCount() {
			return( GetItemCount( "SELECT DbTrack" ));
		}
	}
}
