using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyStream : IPlayExhaustedStrategy {
		private readonly	IInternetStreamProvider	mStreamProvider;

		public PlayExhaustedStrategyStream( IInternetStreamProvider streamProvider ) {
			mStreamProvider = streamProvider;
		}

		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.PlayStream ); }
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
				var stream = mStreamProvider.GetStream( itemId );
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
