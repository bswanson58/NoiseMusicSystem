using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
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

		public bool QueueTracks( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			Condition.Requires( queueMgr ).IsNotNull();

			var retValue = false;

			if( parameters is PlayStrategyParameterDbId ) {
				var dbParam = parameters as PlayStrategyParameterDbId;

				foreach( var item in queueMgr.PlayList ) {
					if(( item.IsStream ) &&
					   ( item.Stream != null ) &&
					   ( item.Stream.DbId == dbParam.DbItemId )) {
						item.HasPlayed = false;

						retValue = true;
					}
				}

				if( !retValue ) {
					var stream = mStreamProvider.GetStream( dbParam.DbItemId );
					if( stream != null ) {
						queueMgr.Add( stream );

						retValue = true;
					}
				}
			}

			return ( retValue );
		}
	}
}
