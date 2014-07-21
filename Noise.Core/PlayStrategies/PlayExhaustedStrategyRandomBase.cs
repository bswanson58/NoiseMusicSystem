using System;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public abstract class PlayExhaustedStrategyRandomBase : PlayExhaustedStrategyBase {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly Random				mRandom;

		protected PlayExhaustedStrategyRandomBase( ePlayExhaustedStrategy strategy, string displayName, string strategyDescription, string parameterName,
												   IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
			base( strategy, displayName, strategyDescription, parameterName ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		protected PlayExhaustedStrategyRandomBase( ePlayExhaustedStrategy strategy, string strategyName, string strategyDescription,
												   IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
			base( strategy, strategyName, strategyDescription ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		protected int NextRandom( int maxValue ) {
			var	retValue = 0;

			if( maxValue > 0 ) {
				retValue = mRandom.Next( maxValue );
			}

			return( retValue );
		}

		protected DbTrack RandomTrackFromArtist( DbArtist artist ) {
			var retValue = default( DbTrack );

			if( artist != null ) {
				using( var albumList = mAlbumProvider.GetAlbumList( artist )) {
					if( albumList.List != null ) {
						var goodList = from album in albumList.List where album.Rating >= 0 select album;

						retValue = RandomTrackFromAlbum( goodList.Skip( NextRandom( artist.AlbumCount - 1 )).Take( 1 ).FirstOrDefault());

					}
				}
			}

			return ( retValue );
		}

		protected DbTrack RandomTrackFromAlbum( DbAlbum album ) {
			var retValue = default( DbTrack );

			if( album != null ) {
				using( var trackList = mTrackProvider.GetTrackList( album )) {
					if( trackList != null ) {
						var goodList = from track in trackList.List where track.Rating >= 0 select track;

						retValue = goodList.Skip( NextRandom( album.TrackCount - 1 )).Take( 1 ).FirstOrDefault();
					}
				}
			}

			return( retValue );
		}
	}
}
