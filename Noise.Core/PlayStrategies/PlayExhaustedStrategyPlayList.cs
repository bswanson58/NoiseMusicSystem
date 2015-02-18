using CuttingEdge.Conditions;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyPlayList : PlayExhaustedStrategyBase {
		private readonly	IPlayListProvider	mPlayListProvider;
		private	readonly	ITrackProvider		mTrackProvider;

		public PlayExhaustedStrategyPlayList( IPlayListProvider playListProvider, ITrackProvider trackProvider, ILogPlayStrategy log ) :
			base( ePlayExhaustedStrategy.PlayList, "Play Playlist...", "Play the tracks from the chosen play list.", "Play List", log ) {
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
			Condition.Requires( PlayQueueMgr ).IsNotNull();

			var retValue = false;

			if( Parameters is PlayStrategyParameterDbId ) {
				var dbParam = Parameters as PlayStrategyParameterDbId;

				var	playList = mPlayListProvider.GetPlayList( dbParam.DbItemId );

				if( playList != null ) {
					var tracks = mTrackProvider.GetTrackListForPlayList( playList );

					foreach( var track in tracks ) {
						if(!PlayQueueMgr.IsTrackQueued( track )) {
							PlayQueueMgr.Add( track );

							LogTrackAdded( track );
							retValue = true;
						}
					}
				}
			}

			return( retValue );
		}
	}
}
