using System.Collections.Generic;
using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class TrackProvider : BaseProvider<DbTrack>, ITrackProvider {

		public TrackProvider( IContextProvider contextProvider ) :
			base( contextProvider ) {
		}

		public void AddTrack( DbTrack track ) {
			using( var context = CreateContext()) {
				context.Set<DbTrack>().Add( track );
				context.SaveChanges();
			}
		}

		public DbTrack GetTrack( long trackId ) {
			return( GetItemByKey( trackId ));
		}

		public IDataProviderList<DbTrack> GetTrackList( long albumId ) {
			var context = CreateContext();

			return( new EfProviderList<DbTrack>( context, context.Set<DbTrack>()));
		}

		public IDataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			var context = CreateContext();

			return( new EfProviderList<DbTrack>( context, from track in context.Set<DbTrack>() where track.Album == forAlbum.DbId select track ));
		}

		public IDataProviderList<DbTrack> GetTrackListForGenre( long genreId ) {
			var context = CreateContext();

			return( new EfProviderList<DbTrack>( context, from track in context.Set<DbTrack>() where track.UserGenre == genreId select track ));
		}

		public IDataProviderList<DbTrack> GetFavoriteTracks() {
			var context = CreateContext();

			return( new EfProviderList<DbTrack>( context, from track in context.Set<DbTrack>() where track.IsFavorite select track ));
		}

		public IDataProviderList<DbTrack> GetNewlyAddedTracks() {
			var context = CreateContext();

			return( new EfProviderList<DbTrack>( context, from track in context.Set<DbTrack>() orderby track.DateAddedTicks select track ));
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public IDataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			var context = CreateContext();

			return( new EfUpdateShell<DbTrack>( context, GetTrack( trackId )));
		}

		public long GetItemCount() {
			return( CreateContext().Set<DbTrack>().Count());
		}
	}
}
