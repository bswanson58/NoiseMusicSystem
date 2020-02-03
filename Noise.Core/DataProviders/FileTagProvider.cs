using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	public class FileTagProvider {
		private readonly ITagManager			mTagManager;
		private readonly IArtworkProvider		mArtworkProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly INoiseLog				mLog;

		public FileTagProvider( ITagManager tagManager, IArtworkProvider artworkProvider, IStorageFolderSupport storageFolderSupport, INoiseLog log ) {
			mArtworkProvider = artworkProvider;
			mStorageFolderSupport = storageFolderSupport;
			mTagManager = tagManager;
			mLog = log;
		}

		public IMetaDataProvider GetProvider( StorageFile storageFile, eAudioEncoding encoding ) {
			IMetaDataProvider	retValue = null;

			switch( encoding ) {
                case eAudioEncoding.AAC:
				case eAudioEncoding.FLAC:
				case eAudioEncoding.MP3:
				case eAudioEncoding.OGG:
				case eAudioEncoding.WMA:
					retValue = new Mp3TagProvider( mArtworkProvider, mTagManager, mStorageFolderSupport, storageFile, mLog );
					break;
			}

			return( retValue );
		}
	}
}
