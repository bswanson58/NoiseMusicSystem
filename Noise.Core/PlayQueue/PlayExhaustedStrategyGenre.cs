using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyGenre : PlayExhaustedListBase {
		private readonly	IDataProvider	mDataProvider;

		public PlayExhaustedStrategyGenre( IDataProvider dataProvider ) {
			mDataProvider = dataProvider;
		}

		protected override void FillTrackList( long itemId ) {
			using( var trackList = mDataProvider.GetGenreTracks( itemId )) {
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
