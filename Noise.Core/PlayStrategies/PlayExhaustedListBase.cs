using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal abstract class PlayExhaustedListBase : IPlayExhaustedStrategy {
		private		readonly ePlayExhaustedStrategy	mStrategy;
		private		readonly string					mDisplayName;
		private		readonly bool					mParametersRequired;
		protected	readonly List<DbTrack>			mTrackList;
		protected	IPlayQueue						mQueueMgr;
		protected	IPlayStrategyParameters			mParameters;
		private		long							mCurrentId;

		protected abstract void FillTrackList( long itemId );
		protected abstract string FormatDescription();

		protected PlayExhaustedListBase( ePlayExhaustedStrategy strategy, string displayName, bool parametersRequired ) {
			mStrategy = strategy;
			mDisplayName = displayName;
			mParametersRequired = parametersRequired;
			mTrackList = new List<DbTrack>();
		}

		public ePlayExhaustedStrategy StrategyId {
			get { return( mStrategy ); }
		}

		public string DisplayName {
			get{ return( mDisplayName ); }
		}

		public string StrategyDescription {
			get {  return( FormatDescription()); }
		}

		public bool	RequiresParameters {
			get{ return( mParametersRequired ); }
		}

		public bool Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			mQueueMgr = queueMgr;
			mParameters = parameters;

			Condition.Requires( mQueueMgr ).IsNotNull();
			if( mParametersRequired ) {
				Condition.Requires( mParameters ).IsOfType( typeof( PlayStrategyParameterDbId ));
			}

			return( true );
		}

		public bool QueueTracks() {
			Condition.Requires( mQueueMgr ).IsNotNull();

			var itemId = Constants.cDatabaseNullOid;

			if( mParameters != null ) {
				Condition.Requires( mParameters ).IsOfType( typeof( PlayStrategyParameterDbId ));

				if( mParameters is PlayStrategyParameterDbId ) {
					var dbParams = mParameters as PlayStrategyParameterDbId;

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

			return( QueueTracks( 3 - mQueueMgr.UnplayedTrackCount ));
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
