using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class UpdateMusicPipelineStep :BasePipelineStep {
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITrackProvider			mTrackProvider;

		public UpdateMusicPipelineStep( IArtistProvider artistProvider, ITrackProvider trackProvider, IStorageFileProvider storageFileProvider ) :
			base( ePipelineStep.UpdateMusic ) {
			mArtistProvider = artistProvider;
			mTrackProvider = trackProvider;
			mStorageFileProvider = storageFileProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();
			Condition.Requires( context.Track ).IsNotNull();
			Condition.Requires( context.Summary ).IsNotNull();

			if(( context.Artist != null ) &&
			   ( context.Album != null )) {
				context.Track.Album = context.Album.DbId;
				mTrackProvider.AddTrack( context.Track );

				using( var updater = mStorageFileProvider.GetFileForUpdate( context.StorageFile.DbId )) {
					if( updater.Item != null ) {
						updater.Item.MetaDataPointer = context.Track.DbId;
						updater.Item.FileType = eFileType.Music;

						updater.Update();
					}
				}

				context.Summary.TracksAdded++;

				using( var updater = mArtistProvider.GetArtistForUpdate( context.Artist.DbId )) {
					if( updater.Item != null ) {
						updater.Item.UpdateLastChange();

						updater.Update();
					}
				}
			}
		}
	}
}
