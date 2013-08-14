using System;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public abstract class PlayExhaustedStrategyRandomBase : PlayExhaustedStrategyBase {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly Random				mRandom;

		protected PlayExhaustedStrategyRandomBase( ePlayExhaustedStrategy strategy, IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
			base( strategy ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		protected int NextRandom( int maxValue ) {
			return( mRandom.Next( maxValue ));
		}

		protected DbTrack RandomTrackFromArtist( DbArtist artist ) {
			var retValue = default( DbTrack );

			if( artist != null ) {
				using( var albumList = mAlbumProvider.GetAlbumList( artist )) {
					if( albumList.List != null ) {
						retValue = RandomTrackFromAlbum( albumList.List.Skip( NextRandom( artist.AlbumCount - 1 )).Take( 1 ).FirstOrDefault());

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
						retValue = trackList.List.Skip( NextRandom( album.TrackCount - 1 )).Take( 1 ).FirstOrDefault();
					}
				}
			}

			return( retValue );
		}
	}
}
