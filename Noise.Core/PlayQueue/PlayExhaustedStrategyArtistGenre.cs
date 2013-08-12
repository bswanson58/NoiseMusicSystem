using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public class PlayExhaustedStrategyArtistGenre : IPlayExhaustedStrategy {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly List<DbArtist>		mArtistList;
		private readonly Random				mRandom;

		public PlayExhaustedStrategyArtistGenre( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mArtistList = new List<DbArtist>();
			mRandom = new Random( DateTime.Now.Millisecond );
		}

		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.PlayArtistGenre ); }
		}

		public bool QueueTracks( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			var retValue = false;

			if( parameters is PlayStrategyParameterDbId ) {
				if( !mArtistList.Any()) {
					var parms = parameters as PlayStrategyParameterDbId;

					LoadArtists( parms.DbItemId );
				}

				if( mArtistList.Any()) {
					var circuitBreaker = 25;

					while( circuitBreaker > 0 ) {
						var track = SelectATrack();

						if(( track != null ) &&
						   ( !queueMgr.IsTrackQueued( track )) &&
						   ( track.Rating >= 0 )) {
							queueMgr.StrategyAdd( track );

							retValue = true;
							break;
						}

						circuitBreaker--;
					}
				}
			}

			return( retValue );
		}

		private DbTrack SelectATrack() {
			var retValue = default( DbTrack );
			var artist = mArtistList.Skip( mRandom.Next( mArtistList.Count - 1 )).Take( 1 ).FirstOrDefault();

			if( artist != null ) {
				using( var albumList = mAlbumProvider.GetAlbumList( artist )) {
					if( albumList.List != null ) {
						var album = albumList.List.Skip( mRandom.Next( artist.AlbumCount - 1 )).Take( 1 ).FirstOrDefault();

						if( album != null ) {
							using( var trackList = mTrackProvider.GetTrackList( album )) {
								if( trackList != null ) {
									retValue = trackList.List.Skip( mRandom.Next( album.TrackCount - 1 )).Take( 1 ).FirstOrDefault();
								}
							}
						}
					}
				}
			}

			return( retValue );
		}

		private void LoadArtists( long genreId ) {
			using( var artists = mArtistProvider.GetArtistList() ) {
				if( artists.List != null ) {
					mArtistList.AddRange( from artist in artists.List where artist.Genre == genreId select artist );
				}
			}
		}
	}
}
