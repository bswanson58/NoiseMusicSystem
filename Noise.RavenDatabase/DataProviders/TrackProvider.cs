using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;
using Raven.Client.Indexes;

namespace Noise.RavenDatabase.DataProviders {
	internal class TracksByDbId : AbstractIndexCreationTask<DbTrack> {
		public TracksByDbId() {
			Map = tracks => from track in tracks select new { track.DbId };
		}
	}

	public class TracksByAlbum : AbstractIndexCreationTask<DbTrack> {
		public TracksByAlbum() {
			Map = tracks => from track in tracks select new { track.Album };
		}
	}

	internal class TrackProvider : BaseProvider<DbTrack>, ITrackProvider {
		public TrackProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
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

		public IDataProviderList<DbTrack> GetTrackList( DbArtist forArtist ) {
			return( Database.Find( track => track.Artist == forArtist.DbId ));
		}

		public IDataProviderList<DbTrack> GetTrackList( long albumId ) {
			return( Database.Find( track => track.Album == albumId, typeof( TracksByAlbum ).Name ));
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

        public IDataProviderList<DbTrack> GetRatedTracks( int ratedAtLeast ) {
            return( Database.Find( track => track.Rating >= ratedAtLeast ));
        }

        public IDataProviderList<DbTrack> GetNewlyAddedTracks() {
			return( new RavenDataProviderList<DbTrack>( Database.FindAll().List.OrderBy( track => track.DateAddedTicks )));
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public ITrackUpdateShell GetTrackForUpdate( long trackId ) {
			return( new RavenTrackUpdateShell( track => Database.Update( track ), Database.Get( trackId )));
		}

		public long GetItemCount() {
			return( Database.Count());
		}
	}
}
