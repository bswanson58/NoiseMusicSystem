using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class TrackProvider : ITrackProvider {
		private readonly NoiseContext	mContext;

		public TrackProvider() {
			mContext = new NoiseContext();
		}

		public void AddTrack( DbTrack track ) {
			mContext.Tracks.Add( track );
			mContext.SaveChanges();
		}

		public DbTrack GetTrack( long trackId ) {
			return( mContext.Tracks.Find( trackId ));
		}

		public DataProviderList<DbTrack> GetTrackList( long albumId ) {
			return( new DataProviderList<DbTrack>( null, mContext.Tracks ));
		}

		public DataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			return( new DataProviderList<DbTrack>( null, from track in mContext.Tracks where track.Album == forAlbum.DbId select track ));
		}

		public DataProviderList<DbTrack> GetTrackListForGenre( long genreId ) {
			return( new DataProviderList<DbTrack>( null, from track in mContext.Tracks where track.UserGenre == genreId select track ));
		}

		public DataProviderList<DbTrack> GetFavoriteTracks() {
			return( new DataProviderList<DbTrack>( null, from track in mContext.Tracks where track.IsFavorite select track ));
		}

		public DataProviderList<DbTrack> GetNewlyAddedTracks() {
			return( new DataProviderList<DbTrack>( null, from track in mContext.Tracks orderby track.DateAddedTicks select track ));
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public DataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			return( new DataUpdateShell<DbTrack>( null, GetTrack( trackId )));
		}

		public long GetItemCount() {
			return( mContext.Tracks.Count());
		}
	}
}
