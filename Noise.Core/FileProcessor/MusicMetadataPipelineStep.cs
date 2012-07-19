using CuttingEdge.Conditions;

namespace Noise.Core.FileProcessor {
	internal class MusicMetadataPipelineStep : BasePipelineStep {
		public MusicMetadataPipelineStep() :
			base( ePipelineStep.AddMusicMetadata ) { }

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.MetaDataProviders ).IsNotEmpty();

			if(( context.Artist != null ) &&
			   ( context.Album != null ) &&
			   ( context.Track != null )) {
				foreach( var provider in context.MetaDataProviders ) {
					provider.AddAvailableMetaData( context.Artist, context.Album, context.Track );
				}
			}
		}
	}
}
