using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyGenre : PlayExhaustedListBase {
		public PlayExhaustedStrategyGenre( IUnityContainer container ) :
			base( container ) {
		}

		protected override void FillTrackList( long itemId ) {
			var	noiseManager = mContainer.Resolve<INoiseManager>();

			using( var trackList = noiseManager.DataProvider.GetGenreTracks( itemId )) {
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
