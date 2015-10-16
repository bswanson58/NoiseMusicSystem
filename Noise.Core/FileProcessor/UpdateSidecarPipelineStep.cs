using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class UpdateSidecarPipelineStep : BasePipelineStep {
		private readonly ISidecarProvider		mSidecarProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;

		public UpdateSidecarPipelineStep( ISidecarProvider sidecarProvider, IStorageFileProvider storageFileProvider ) :
			base( ePipelineStep.UpdateSidecar ) {
			mSidecarProvider = sidecarProvider;
			mStorageFileProvider = storageFileProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			if( context.Artist != null ) {
				var sidecar = context.Album != null ? new StorageSidecar( context.StorageFile.Name, context.Album )
													: new StorageSidecar( context.StorageFile.Name, context.Artist );
				mSidecarProvider.Add( sidecar );

				using( var updater = mStorageFileProvider.GetFileForUpdate( context.StorageFile.DbId )) {
					if( updater.Item != null ) {
						updater.Item.MetaDataPointer = context.Album != null ? context.Album.DbId : context.Artist.DbId;
						updater.Item.FileType = eFileType.Sidecar;
						updater.Item.WasUpdated = false;

						updater.Update();
					}
				}
			}
		}
	}
}
