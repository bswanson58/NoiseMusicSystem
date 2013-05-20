using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TrackProvider : BaseProvider<DbTrack>, ITrackProvider {
		public TrackProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddTrack( DbTrack track ) {
			Database.Add( track );
		}

		public DbTrack GetTrack( long trackId ) {
			return( Database.Get( trackId ));
		}

		public void DeleteTrack( DbTrack track ) {
			Database.Delete( track );
		}

		public IDataProviderList<DbTrack> GetTrackList( long albumId ) {
			return( Database.Find( track => track.Album == albumId ));
		}

		public IDataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			return( GetTrackList( forAlbum.DbId ));
		}

		public IDataProviderList<DbTrack> GetTrackListForGenre( long genreId ) {
			return( Database.Find( track => track.UserGenre == genreId ));
		}

		public IDataProviderList<DbTrack> GetFavoriteTracks() {
			return( Database.Find( track => track.IsFavorite ));
		}

		public IDataProviderList<DbTrack> GetNewlyAddedTracks() {
//			return( new RavenDataProviderList<DbTrack>( Database.FindAll().Query().OrderBy( track => track.DateAddedTicks ));
			return( null );
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public IDataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			return( new RavenDataUpdateShell<DbTrack>( track => Database.Update( track ), Database.Get( trackId )));
		}

		public long GetItemCount() {
			return( Database.Count());
		}
	}
}
