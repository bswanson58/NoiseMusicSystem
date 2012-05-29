using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class TrackProvider : BaseProvider<DbTrack>, ITrackProvider {

		public TrackProvider( IContextProvider contextProvider ) :
			base( contextProvider ) {
		}

		public void AddTrack( DbTrack track ) {
			AddItem( track );
		}

		public void DeleteTrack( DbTrack track ) {
			RemoveItem( track );
		}

		public DbTrack GetTrack( long trackId ) {
			return( GetItemByKey( trackId ));
		}

		public IDataProviderList<DbTrack> GetTrackList( long albumId ) {
			var context = CreateContext();

			return( new EfProviderList<DbTrack>( context, Set( context ).Where( entity => entity.Album == albumId )));
		}

		public IDataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			return( GetTrackList( forAlbum.DbId ));
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
			Condition.Requires( playList ).IsNotNull();

			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public IDataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			var context = CreateContext();

			return( new EfUpdateShell<DbTrack>( context, GetItemByKey( context, trackId )));
		}

		public long GetItemCount() {
			return( GetEntityCount());
		}
	}
}
