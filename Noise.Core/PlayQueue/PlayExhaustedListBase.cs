using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal abstract class PlayExhaustedListBase : IPlayExhaustedStrategy {
		private readonly ePlayExhaustedStrategy	mStrategy;
		protected readonly List<DbTrack>		mTrackList;
		protected IPlayQueue					mQueueMgr;
		private		long						mCurrentId;

		protected abstract void FillTrackList( long itemId );

		protected PlayExhaustedListBase( ePlayExhaustedStrategy strategy ) {
			mStrategy = strategy;
			mTrackList = new List<DbTrack>();
		}

		public ePlayExhaustedStrategy PlayStrategy {
			get { return( mStrategy ); }
		}

		public bool QueueTracks( IPlayQueue queueMgr, long itemId ) {
			Condition.Requires( queueMgr ).IsNotNull();

			var retValue = false;

			if( mCurrentId != itemId ) {
				mTrackList.Clear();
				mCurrentId = itemId;
			}

			if( queueMgr != null ) {
				mQueueMgr = queueMgr;

				if(!mTrackList.Any()) {
					FillTrackList( itemId );
				}

				retValue =  QueueTracks( 3 - mQueueMgr.UnplayedTrackCount );
			}

			return( retValue );
		}

		private bool QueueTracks( int count ) {
			var retValue= false;

			for( int x = 0; x < count; x++ ) {
				if( mTrackList.Count > 0 ) {
					mQueueMgr.StrategyAdd( SelectTrack());

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
