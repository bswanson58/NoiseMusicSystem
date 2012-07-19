using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class UpdateUndeterminedPipelineStep : BasePipelineStep {
		private readonly IStorageFileProvider mStorageFileProvider;

		public UpdateUndeterminedPipelineStep( IStorageFileProvider storageFileProvider ) :
			base( ePipelineStep.UpdateUndetermined ) {
			mStorageFileProvider = storageFileProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			using( var updater = mStorageFileProvider.GetFileForUpdate( context.StorageFile.DbId )) {
				if( updater.Item != null ) {
					updater.Item.FileType = eFileType.Undetermined;

					updater.Update();
				}
			}
		}
	}
}
