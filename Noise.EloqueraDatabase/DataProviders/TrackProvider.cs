using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class TrackProvider : BaseDataProvider<DbTrack>, ITrackProvider {
		private readonly IRootFolderProvider	mRootFolderProvider;
		private long							mInitialScanCompleted;

		public TrackProvider( IEloqueraManager databaseManager, IRootFolderProvider rootFolderProvider ) :
			base( databaseManager ) {
			mRootFolderProvider = rootFolderProvider;
		}

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

		public IDataProviderList<DbTrack> GetTrackList( DbArtist forArtist ) {
			return( TryGetList( "SELECT DbTrack WHERE Artist = @artistId", new Dictionary<string, object>{{ "artistId", forArtist.DbId }}, "GetTrackList" ));
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
			if( mInitialScanCompleted == 0 ) {
				using( var folderList = mRootFolderProvider.GetRootFolderList()) {
					foreach( var folder in folderList.List ) {
						if( folder.InitialScanCompleted > mInitialScanCompleted ) {
							mInitialScanCompleted = folder.InitialScanCompleted;
						}
					}
				}
			}

			return( TryGetList( "SELECT DbTrack Where DateAddedTicks > @initialScan ORDER BY DateAddedTicks DESC",
									new Dictionary<string, object>{{ "initialScan", mInitialScanCompleted }}, "GetNewlyAddedTracks" ));
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
