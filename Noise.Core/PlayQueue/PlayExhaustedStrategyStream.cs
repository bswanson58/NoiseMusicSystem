using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyStream : IPlayExhaustedStrategy {
		private readonly IUnityContainer	mContainer;

		public PlayExhaustedStrategyStream( IUnityContainer container ) {
			mContainer = container;
		}
		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			var retValue = false;

			foreach( var item in queueMgr.PlayList ) {
				if(( item.IsStream ) &&
				   ( item.Stream != null ) &&
				   ( item.Stream.DbId == itemId ) ) {
					item.HasPlayed = false;

					retValue = true;
				}
			}

			if( !retValue ) {
				var noiseManager = mContainer.Resolve<INoiseManager>();
				var stream = noiseManager.DataProvider.GetStream( itemId );
				if( stream != null ) {
					queueMgr.Add( stream );

					retValue = true;
				}
			}
			return ( retValue );
		}

		public void NextTrackPlayed() {
		}
	}
}
