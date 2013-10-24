using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyPlayList : PlayExhaustedStrategyBase {
		private readonly	IPlayListProvider	mPlayListProvider;
		private	readonly	ITrackProvider		mTrackProvider;

		public PlayExhaustedStrategyPlayList( IPlayListProvider playListProvider, ITrackProvider trackProvider ) :
			base( ePlayExhaustedStrategy.PlayList, "Play Playlist...", true, "Play List" ) {
			mPlayListProvider = playListProvider;
			mTrackProvider = trackProvider;
		}

		protected override string FormatDescription() {
			return( string.Format( "play tracks from a play list" ));
		}

		protected override DbTrack SelectATrack() {
			throw new System.NotImplementedException();
		}

		public override bool QueueTracks() {
			Condition.Requires( mQueueMgr ).IsNotNull();

			var retValue = false;

			if( mParameters is PlayStrategyParameterDbId ) {
				var dbParam = mParameters as PlayStrategyParameterDbId;

				var	playList = mPlayListProvider.GetPlayList( dbParam.DbItemId );

				if( playList != null ) {
					var tracks = mTrackProvider.GetTrackListForPlayList( playList );

					foreach( var track in tracks ) {
						if(!mQueueMgr.IsTrackQueued( track )) {
							mQueueMgr.Add( track );

							retValue = true;
						}
					}
				}
			}

			return( retValue );
		}
	}
}
