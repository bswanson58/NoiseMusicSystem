using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class UpdateSidecarPipelineStep : BasePipelineStep {
		private readonly ISidecarProvider	mSidecarProvider;

		public UpdateSidecarPipelineStep( ISidecarProvider sidecarProvider ) :
			base( ePipelineStep.UpdateSidecar ) {
			mSidecarProvider = sidecarProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			if(( context.Artist != null ) &&
			   ( context.Album != null )) {
				mSidecarProvider.Add( new StorageSidecar( context.StorageFile.Name, context.Album ));
			}
		}
	}
}
