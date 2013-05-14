using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TrackProvider : ITrackProvider {
		private readonly IDbFactory				mDbFactory;
		private readonly IRepository<DbTrack>	mDatabase;

		public TrackProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepository<DbTrack>( mDbFactory.GetLibraryDatabase(), track => new object[] { track.DbId });
		}

		public void AddTrack( DbTrack track ) {
			mDatabase.Add( track );
		}

		public DbTrack GetTrack( long trackId ) {
			return( mDatabase.Get( trackId ));
		}

		public void DeleteTrack( DbTrack track ) {
			mDatabase.Delete( track );
		}

		public IDataProviderList<DbTrack> GetTrackList( long albumId ) {
			return( new RavenDataProviderList<DbTrack>( mDatabase.Find( track => track.Album == albumId )));
		}

		public IDataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			return( GetTrackList( forAlbum.DbId ));
		}

		public IDataProviderList<DbTrack> GetTrackListForGenre( long genreId ) {
			return( new RavenDataProviderList<DbTrack>( mDatabase.Find( track => track.UserGenre == genreId )));
		}

		public IDataProviderList<DbTrack> GetFavoriteTracks() {
			return( new RavenDataProviderList<DbTrack>( mDatabase.Find( track => track.IsFavorite )));
		}

		public IDataProviderList<DbTrack> GetNewlyAddedTracks() {
//			return( new RavenDataProviderList<DbTrack>( mDatabase.FindAll().Query().OrderBy( track => track.DateAddedTicks ));
			return( null );
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public IDataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			return( new RavenDataUpdateShell<DbTrack>( track => mDatabase.Update( track ), mDatabase.Get( trackId )));
		}

		public long GetItemCount() {
			return( mDatabase.Count());
		}
	}
}
