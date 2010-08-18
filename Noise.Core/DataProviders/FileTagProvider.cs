using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public class FileTagProvider {
		private readonly IDatabaseManager	mDatabase;

		public FileTagProvider( IDatabaseManager database ) {
			mDatabase = database;
		}

		public IMetaDataProvider GetProvider( StorageFile storageFile, eAudioEncoding encoding ) {
			IMetaDataProvider	retValue = null;

			switch( encoding ) {
				case eAudioEncoding.FLAC:
				case eAudioEncoding.MP3:
				case eAudioEncoding.OGG:
				case eAudioEncoding.WMA:
					retValue = new Mp3TagProvider( mDatabase, storageFile );
					break;
			}

			return( retValue );
		}
	}
}
