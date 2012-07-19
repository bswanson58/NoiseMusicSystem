using CuttingEdge.Conditions;

namespace Noise.Core.FileProcessor {
	internal class DetermineVolumePipelineStep : BasePipelineStep {
		public DetermineVolumePipelineStep() :
			base( ePipelineStep.DetermineVolume ) { }

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.Track ).IsNotNull();
			Condition.Requires( context.MetaDataProviders ).IsNotEmpty();

			foreach( var provider in context.MetaDataProviders ) {
				context.Track.VolumeName = provider.VolumeName;

				if(!string.IsNullOrWhiteSpace( context.Track.VolumeName )) {
					break;
				}
			}
		}
	}
}
