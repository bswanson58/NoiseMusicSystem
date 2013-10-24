using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyStream : PlayExhaustedStrategyBase {
		private readonly	IInternetStreamProvider	mStreamProvider;
		private string		mStreamName;

		public PlayExhaustedStrategyStream( IInternetStreamProvider streamProvider ) :
			base( ePlayExhaustedStrategy.PlayStream, "Play Radio Station...", true, "Stream" ) {
			mStreamProvider = streamProvider;
		}

		protected override string FormatDescription() {
			return( string.Format( "play radio station {0}", mStreamName ));
		}

		protected override DbTrack SelectATrack() {
			throw new System.NotImplementedException();
		}

		protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
			if( mParameters is PlayStrategyParameterDbId ) {
				var dbParam = mParameters as PlayStrategyParameterDbId;
				var stream = mStreamProvider.GetStream( dbParam.DbItemId );

				if( stream != null ) {
					mStreamName = stream.Name;
				}
			}
		}

		public override bool QueueTracks() {
			Condition.Requires( mQueueMgr ).IsNotNull();

			var retValue = false;

			if( mParameters is PlayStrategyParameterDbId ) {
				var dbParam = mParameters as PlayStrategyParameterDbId;

				foreach( var item in mQueueMgr.PlayList ) {
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
						mQueueMgr.Add( stream );

						retValue = true;
					}
				}
			}

			return ( retValue );
		}
	}
}
