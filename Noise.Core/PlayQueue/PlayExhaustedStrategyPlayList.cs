using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyPlayList : IPlayExhaustedStrategy {
		private readonly IUnityContainer	mContainer;

		public PlayExhaustedStrategyPlayList( IUnityContainer container ) {
			mContainer = container;
		}

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			var retValue = false;
			var noiseManager = mContainer.Resolve<INoiseManager>();
			var	playList = noiseManager.PlayListMgr.GetPlayList( itemId );

			if( playList != null ) {
				var tracks = noiseManager.PlayListMgr.GetTracks( playList );

				foreach( var track in tracks ) {
					if(!queueMgr.IsTrackQueued( track )) {
						queueMgr.Add( track );

						retValue = true;
					}
				}
			}
			return( retValue );
		}

		public void NextTrackPlayed() {
		}
	}
}
