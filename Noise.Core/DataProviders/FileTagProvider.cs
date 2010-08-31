using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public class FileTagProvider {
		private readonly IUnityContainer	mContainer;

		public FileTagProvider( IUnityContainer container ) {
			mContainer = container;
		}

		public IMetaDataProvider GetProvider( StorageFile storageFile, eAudioEncoding encoding ) {
			IMetaDataProvider	retValue = null;

			switch( encoding ) {
				case eAudioEncoding.FLAC:
				case eAudioEncoding.MP3:
				case eAudioEncoding.OGG:
				case eAudioEncoding.WMA:
					retValue = new Mp3TagProvider( mContainer, storageFile );
					break;
			}

			return( retValue );
		}
	}
}
