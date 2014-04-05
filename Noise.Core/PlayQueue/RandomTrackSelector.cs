using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public class RandomTrackSelector : IRandomTrackSelector {
		private readonly ITrackProvider		mTrackProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly Random				mRandom;

		public RandomTrackSelector( IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		public IEnumerable<DbTrack> SelectTracks( DbArtist fromArtist, Func<DbTrack, bool> approveTrack, int count ) {
			IEnumerable<DbTrack>	retValue;

			using( var albums = mAlbumProvider.GetAlbumList( fromArtist )) {
				retValue = SelectRandomTracks( albums.List, approveTrack, count );
			}

			return( retValue );
		}

		public IEnumerable<DbTrack> SelectTracks( IEnumerable<DbAlbum> albumList, Func<DbTrack, bool> approveTrack, int count ) {
			return( SelectRandomTracks( albumList, approveTrack, count ));
		}

		public IEnumerable<DbTrack> SelectTracks( IEnumerable<SearchResultItem> searchList, Func<DbTrack, bool> approveTrack, int count ) {
			var retValue = new List<DbTrack>();

			if( searchList != null ) {
				var albumList = new List<DbAlbum>();
				var trackList = new List<DbTrack>();

				foreach( var item in searchList ) {
					if( item.Track != null ) {
						trackList.Add( item.Track );
					}
					else {
						if( item.Album != null ) {
							albumList.Add( item.Album );
						}
					}
				}

				if( albumList.Count > 0 ) {
					trackList.AddRange( SelectRandomTracks( albumList, track => true, Math.Max( trackList.Count, count * 3 )));
				}

				if( trackList.Any()) {
					retValue.AddRange( SelectRandomTracks( trackList, approveTrack, count ));
				}
			}

			return( retValue );
		}

		private IEnumerable<DbTrack> SelectRandomTracks( IEnumerable<DbAlbum> albums, Func<DbTrack, bool> approveTrack, int count ) {
			var retValue = new List<DbTrack>();
			var albumList = albums.ToList();
			int	albumCount = albumList.Count();

			if( albumCount > 0 ) {
				int	circuitBreaker = 0;

				while(( retValue.Count < count ) &&
					  ( circuitBreaker < ( count * 3 ))) {
					var album = albumList.Skip( NextRandom( albumCount - 1 )).Take( 1 ).FirstOrDefault();

					if( album != null ) {
						var track = RandomTrackFromAlbum( album );

						if(( track.Rating >= 0 ) &&
						   ( retValue.FirstOrDefault( selectedTrack => selectedTrack.DbId == track.DbId ) == null ) &&
						   ( approveTrack(track))) {
							retValue.Add( track );
						}
					}

					circuitBreaker++;
				}
			}

			return( retValue );
		}

		private IEnumerable<DbTrack> SelectRandomTracks( List<DbTrack> trackList, Func<DbTrack, bool> approveTrack, int count ) {
			var retValue = new List<DbTrack>();
			int	circuitBreaker = 0;

			while(( retValue.Count < count ) &&
				  ( circuitBreaker < ( count * 3 ))) {
				var track = trackList.Skip( NextRandom( trackList.Count - 1 )).Take( 1 ).FirstOrDefault();

				if(( track != null ) &&
				   ( approveTrack( track ))) {
					retValue.Add( track );
				}

				circuitBreaker++;
			}

			return( retValue );
		} 

		protected DbTrack RandomTrackFromAlbum( DbAlbum album ) {
			var retValue = default( DbTrack );

			if( album != null ) {
				using( var trackList = mTrackProvider.GetTrackList( album )) {
					if( trackList != null ) {
						var minimumTrackDuration = new TimeSpan( 0, 0, 30 );
						var goodList = from track in trackList.List where track.Rating >= 0 && track.Duration > minimumTrackDuration select track;

						retValue = goodList.Skip( NextRandom( album.TrackCount - 1 )).Take( 1 ).FirstOrDefault();
					}
				}
			}

			return( retValue );
		}

		protected int NextRandom( int maxValue ) {
			return( mRandom.Next( maxValue ));
		}
	}
}
