using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public class PlayStrategyFeaturedArtists : IPlayStrategy {
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ITrackProvider					mTrackProvider;
		private readonly List<long>						mArtistList;
		private readonly Dictionary<long, List<long>>	mTracks;
		private long									mLastArtistPlayed;
		private int										mNextFeaturedPlay;
		private readonly Random							mRandom;
 
		public PlayStrategyFeaturedArtists( IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mArtistList = new List<long>();
			mTracks = new Dictionary<long, List<long>>();
			mRandom = new Random( DateTime.Now.Millisecond );

			mLastArtistPlayed = Constants.cDatabaseNullOid;
		}

		public PlayQueueTrack NextTrack( IPlayQueue queueMgr, IList<PlayQueueTrack> queue, IPlayStrategyParameters parameters ) {
			var retValue = queue.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed ));

			SyncArtistList( parameters );
			mNextFeaturedPlay--;

			if( mNextFeaturedPlay < 0 ) {
				mNextFeaturedPlay = NextPlayInterval();

                var nextArtist = SelectNextArtist( mLastArtistPlayed );
				var tracks = TracksForArtist( nextArtist );

				if( tracks.Any()) {
					var trackIndex = mRandom.Next( tracks.Count - 1 );
					var track = mTrackProvider.GetTrack( tracks[trackIndex] );

					if( track != null ) {
						queueMgr.StrategyAdd( track, retValue );
					}
				}
			}


			return( retValue );
		}

		private void SyncArtistList( IPlayStrategyParameters parameters ) {
			if( parameters is PlayStrategyParameterDbId ) {
				var playParams = parameters as PlayStrategyParameterDbId;
				var updateList = false;

				if( mArtistList.Count == 1 ) {
					if( mArtistList.Any( artistId => artistId != playParams.DbItemId )) {
						updateList = true;
					}
				}
				else {
					updateList = true;
				}

				if( updateList ) {
					mArtistList.Clear();
					mArtistList.Add( playParams.DbItemId );
					mLastArtistPlayed = Constants.cDatabaseNullOid;

					mTracks.Clear();
					mNextFeaturedPlay = NextPlayInterval();
				}
			}
		}

		private long SelectNextArtist( long lastArtist ) {
			var retValue = lastArtist;

			if( mArtistList.Count > 0 ) {
				var index = mArtistList.FindIndex( artist => artist == lastArtist ) + 1;

				if(( index > 0 ) &&
				   ( index < mArtistList.Count )) {
					retValue = mArtistList[index];
				}
				else {
					retValue = mArtistList[0];
				}
			}

			return( retValue );
		}

		private List<long> TracksForArtist( long artistId ) {
			List<long>	retValue;

			if( mTracks.ContainsKey( artistId )) {
				retValue = mTracks[artistId];
			}
			else {
				retValue = new List<long>(); 

				using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
					if( albumList.List != null ) {
						foreach( var album in albumList.List ) {
							using( var trackList = mTrackProvider.GetTrackList( album )) {
								if( trackList.List != null ) {
									retValue.AddRange( trackList.List.Where( track => track.Rating >= 0 ).Select( track => track.DbId ));
								}
							}

							if( retValue.Count > 1000 ) {
								break;
							}
						}
					}
				}

				mTracks[artistId] = retValue;
			}

			return( retValue );
		}
 
		private int NextPlayInterval() {
			return( mRandom.Next( 7 ) + 3 );
		}
	}
}
