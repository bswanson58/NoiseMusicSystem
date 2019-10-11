using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class TrackProvider : BaseProvider<DbTrack>, ITrackProvider {
		private readonly IRootFolderProvider	mRootFolderProvider;
        private readonly IAlbumProvider         mAlbumProvider;

		public TrackProvider( IAlbumProvider albumProvider, IContextProvider contextProvider, IRootFolderProvider rootFolderProvider, ILogDatabase log ) :
			base( contextProvider, log ) {
			mRootFolderProvider = rootFolderProvider;
            mAlbumProvider = albumProvider;
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

		public IDataProviderList<DbTrack> GetTrackList( DbArtist forArtist ) {
			var context = CreateContext();

			return( new EfProviderList<DbTrack>( context, Set( context ).Where( entity => entity.Artist == forArtist.DbId )));
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

        public IDataProviderList<DbTrack> GetRatedTracks( int ratedAtLeast ) {
            var context = CreateContext();

            return( new EfProviderList<DbTrack>( context, from track in context.Set<DbTrack>() where track.Rating >= ratedAtLeast select track ));
        }

        public IDataProviderList<DbTrack> GetNewlyAddedTracks() {
			var context = CreateContext();
			var firstScanCompleted = mRootFolderProvider.FirstScanCompleted();

			return( new EfProviderList<DbTrack>( context, from track in context.Set<DbTrack>() 
														  where track.DateAddedTicks > firstScanCompleted 
														  orderby track.DateAddedTicks descending 
														  select track ));
		}

		public IEnumerable<DbTrack> GetTrackListForPlayList( DbPlayList playList ) {
			Condition.Requires( playList ).IsNotNull();

			return( playList.TrackIds.Select( GetTrack ).ToList());
		}

		public ITrackUpdateShell GetTrackForUpdate( long trackId ) {
			var context = CreateContext();

			return( new TrackUpdateShell( mAlbumProvider, context, GetItemByKey( context, trackId )));
		}

		public long GetItemCount() {
			return( GetEntityCount());
		}
	}
}
