using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal abstract class PlayExhaustedListBase : PlayExhaustedStrategyBase {
		protected	readonly List<DbTrack>		mTrackList;
		private		long						mCurrentId;

		protected abstract void FillTrackList( long itemId );

		protected PlayExhaustedListBase( ePlayExhaustedStrategy strategy, string displayName, string strategyDescription ) :
			base( strategy, displayName, strategyDescription ) {
			mTrackList = new List<DbTrack>();
		}

		protected override DbTrack SelectATrack() {
			throw new NotImplementedException();
		}

		public override bool QueueTracks() {
			Condition.Requires( PlayQueueMgr ).IsNotNull();

			var itemId = Constants.cDatabaseNullOid;

			if( Parameters != null ) {
				Condition.Requires( Parameters ).IsOfType( typeof( PlayStrategyParameterDbId ));

				if( Parameters is PlayStrategyParameterDbId ) {
					var dbParams = Parameters as PlayStrategyParameterDbId;

					itemId = dbParams.DbItemId;
				}
			}

			if( mCurrentId != itemId ) {
				mTrackList.Clear();
				mCurrentId = itemId;
			}

			if( !mTrackList.Any()) {
				FillTrackList( itemId );
			}

			return( QueueTracks( 3 - PlayQueueMgr.UnplayedTrackCount ));
		}

		private bool QueueTracks( int count ) {
			var retValue= false;

			for( int x = 0; x < count; x++ ) {
				if( mTrackList.Count > 0 ) {
					PlayQueueMgr.StrategyAdd( SelectTrack());

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
