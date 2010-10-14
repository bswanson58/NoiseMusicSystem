using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal abstract class PlayExhaustedListBase : IPlayExhaustedStrategy {
		protected readonly IUnityContainer	mContainer;
		protected readonly ILog				mLog;
		protected readonly List<DbTrack>	mTrackList;
		protected IPlayQueue				mQueueMgr;

		protected PlayExhaustedListBase( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
			mTrackList = new List<DbTrack>();
		}

		protected abstract void FillTrackList();

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			mQueueMgr = queueMgr;

			if( !mQueueMgr.StrategyRequestsQueued ) {
				FillTrackList();
			}

			return ( QueueTracks( 3 ) );
		}

		public void NextTrackPlayed() {
			if( ( mQueueMgr != null ) &&
			   ( mQueueMgr.StrategyRequestsQueued ) ) {
				var	trackCount = mQueueMgr.UnplayedTrackCount;

				if( trackCount < 3 ) {
					QueueTracks( 3 - trackCount );
				}
			}
		}

		protected bool QueueTracks( int count ) {
			var retValue= false;

			for( int x = 0; x < count; x++ ) {
				if( mTrackList.Count > 0 ) {
					mQueueMgr.StrategyAdd( SelectTrack() );

					retValue = true;
				}
			}

			return ( retValue );
		}

		private DbTrack SelectTrack() {
			DbTrack	retValue;

			if( mTrackList.Count > 1 ) {
				var r = new Random( DateTime.Now.Millisecond );
				var next = r.Next( mTrackList.Count - 1 );

				retValue = mTrackList.Skip( next ).FirstOrDefault();
				mTrackList.Remove( retValue );
			}
			else {
				retValue = mTrackList[0];
				mTrackList.Remove( retValue );
			}

			return ( retValue );
		}

	}
}
