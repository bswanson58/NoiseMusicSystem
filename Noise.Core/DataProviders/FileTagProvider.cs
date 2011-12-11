using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	public class FileTagProvider {
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ITagManager		mTagManager;

		public FileTagProvider( IDatabaseManager databaseManager, ITagManager tagManager ) {
			mDatabaseManager = databaseManager;
			mTagManager = tagManager;
		}

		public IMetaDataProvider GetProvider( StorageFile storageFile, eAudioEncoding encoding ) {
			IMetaDataProvider	retValue = null;

			switch( encoding ) {
				case eAudioEncoding.FLAC:
				case eAudioEncoding.MP3:
				case eAudioEncoding.OGG:
				case eAudioEncoding.WMA:
					retValue = new Mp3TagProvider( mDatabaseManager, mTagManager, storageFile );
					break;
			}

			return( retValue );
		}
	}
}
