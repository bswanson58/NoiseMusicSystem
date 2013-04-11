using System.IO;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class UpdateInfoPipelineStep : BasePipelineStep {
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITextInfoProvider		mTextInfoProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;

		public UpdateInfoPipelineStep( IArtistProvider artistProvider, ITextInfoProvider textInfoProvider,
									   IStorageFileProvider storageFileProvider, IStorageFolderSupport storageFolderSupport ) :
			base( ePipelineStep.UpdateInfo) {
			mArtistProvider = artistProvider;
			mTextInfoProvider = textInfoProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			if(( context.Artist != null ) &&
			   ( context.Album != null )) {
				var infoPath = mStorageFolderSupport.GetPath( context.StorageFile );

				if( context.StorageFile.WasUpdated ) {
					mTextInfoProvider.UpdateTextInfo( context.StorageFile.MetaDataPointer, infoPath );
				}
				else {
					var	info = new DbTextInfo( context.StorageFile.DbId, ContentType.TextInfo ) {
												Artist = context.Artist.DbId, Album = context.Album.DbId,
												Source = InfoSource.File, FolderLocation = context.StorageFile.ParentFolder,
												IsContentAvailable = true, Name = Path.GetFileNameWithoutExtension( context.StorageFile.Name ) };

					mTextInfoProvider.AddTextInfo( info, infoPath );
					context.StorageFile.MetaDataPointer = info.DbId;
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
						updater.Item.FileType = eFileType.Text;
						updater.Item.WasUpdated = false;

						updater.Update();
					}
				}
			}
		}
	}
}
