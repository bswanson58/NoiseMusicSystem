using CuttingEdge.Conditions;

namespace Noise.Core.FileProcessor {
	internal class DetermineTrackPipelineStep : BasePipelineStep {
		public DetermineTrackPipelineStep() :
			base( ePipelineStep.DetermineTrackName ) {
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.Track ).IsNotNull();
			Condition.Requires( context.MetaDataProviders ).IsNotEmpty();

			foreach( var provider in context.MetaDataProviders ) {
				context.Track.Name = provider.TrackName;

				if(!string.IsNullOrWhiteSpace( context.Track.Name )) {
					break;
				}
			}

			if( string.IsNullOrWhiteSpace( context.Track.Name )) {
				context.Log.LogTrackNotFound( context.StorageFile );
			}
			else {
				context.Log.LogTrackAdded( context.StorageFile, context.Track );
			}
		}
	}
}
