using System.IO;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class UpdateArtworkPipelineStep : BasePipelineStep {
		private readonly IArtistProvider		mArtistProvider;
		private readonly IArtworkProvider		mArtworkProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;

		public UpdateArtworkPipelineStep( IArtistProvider artistProvider, IArtworkProvider artworkProvider,
										  IStorageFileProvider storageFileProvider, IStorageFolderSupport storageFolderSupport ) :
			base( ePipelineStep.UpdateArtwork ) {
			mArtistProvider = artistProvider;
			mArtworkProvider = artworkProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			if(( context.Artist != null ) &&
			   ( context.Album != null )) {
				var	imagePath = mStorageFolderSupport.GetPath( context.StorageFile );

				if( context.StorageFile.WasUpdated ) {
					mArtworkProvider.UpdateArtworkImage( context.StorageFile.MetaDataPointer, imagePath );
				}
				else {
					var artwork = new DbArtwork( context.StorageFile.DbId, mStorageFolderSupport.IsCoverFile( context.StorageFile.Name ) ? ContentType.AlbumCover : 
																																		   ContentType.AlbumArtwork ) {
												Artist = context.Artist.DbId, Album = context.Album.DbId,
												Source = InfoSource.File, FolderLocation = context.StorageFile.ParentFolder, IsContentAvailable = true,
												Name = Path.GetFileNameWithoutExtension( context.StorageFile.Name )};

					mArtworkProvider.AddArtwork( artwork, imagePath );
					context.StorageFile.MetaDataPointer = artwork.DbId;
					context.Log.LogArtworkAdded( context.StorageFile, artwork );
				}

				using( var updater = mArtistProvider.GetArtistForUpdate( context.Artist.DbId )) {
					if( updater.Item != null ) {
						updater.Item.UpdateLastChange();

						updater.Update();
					}
				}

				using( var updater = mStorageFileProvider.GetFileForUpdate( context.StorageFile.DbId )) {
					if( updater.Item != null ) {
						updater.Item.MetaDataPointer = context.StorageFile.MetaDataPointer;
						updater.Item.FileType = eFileType.Picture;
						updater.Item.WasUpdated = false;

						updater.Update();
					}
				}
			}
		}
	}
}
