using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	public class FileTagProvider {
		private readonly ITagManager		mTagManager;
		private readonly IArtworkProvider		mArtworkProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;

		public FileTagProvider( ITagManager tagManager, IArtworkProvider artworkProvider, IStorageFileProvider storageFileProvider ) {
			mArtworkProvider = artworkProvider;
			mStorageFileProvider = storageFileProvider;
			mTagManager = tagManager;
		}

		public IMetaDataProvider GetProvider( StorageFile storageFile, eAudioEncoding encoding ) {
			IMetaDataProvider	retValue = null;

			switch( encoding ) {
				case eAudioEncoding.FLAC:
				case eAudioEncoding.MP3:
				case eAudioEncoding.OGG:
				case eAudioEncoding.WMA:
					retValue = new Mp3TagProvider( mArtworkProvider, mTagManager, mStorageFileProvider, storageFile );
					break;
			}

			return( retValue );
		}
	}
}
