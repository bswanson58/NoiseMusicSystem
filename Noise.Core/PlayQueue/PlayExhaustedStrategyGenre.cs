using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyGenre : PlayExhaustedListBase {
		private readonly ITrackProvider	mTrackProvider;

		public PlayExhaustedStrategyGenre( ITrackProvider trackProvider ) :
			base( ePlayExhaustedStrategy.PlayGenre ) {
			mTrackProvider = trackProvider;
		}

		protected override void FillTrackList( long itemId ) {
			using( var trackList = mTrackProvider.GetTrackListForGenre( itemId )) {
				var	maxTracks = 250;

				foreach( var track in trackList.List ) {
					if(!mQueueMgr.IsTrackQueued( track )) {
						mTrackList.Add( track );
					}

					maxTracks--;
					if( maxTracks == 0 ) {
						break;
					}
				}
			}
		}
	}
}
