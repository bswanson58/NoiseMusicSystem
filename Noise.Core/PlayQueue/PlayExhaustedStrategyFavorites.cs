using System;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyFavorites : PlayExhaustedListBase {
		public PlayExhaustedStrategyFavorites( IUnityContainer container ) :
			base( container ) { }

		protected override void FillTrackList( long itemId ) {
			mTrackList.Clear();

			try {
				var manager = mContainer.Resolve<INoiseManager>();

				using( var list = manager.DataProvider.GetFavoriteTracks()) {
					foreach( var track in list.List ) {
						if(!mQueueMgr.IsTrackQueued( track )) {
							mTrackList.Add( track );
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - PlayExhaustedStrategyFavorites: ", ex );
			}
		}
	}
}
