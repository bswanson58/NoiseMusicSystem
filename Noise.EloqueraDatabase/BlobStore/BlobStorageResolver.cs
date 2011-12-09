using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.BlobStore {
	internal class BlobStorageResolver : IBlobStorageResolver {
		private const string	cDefaultLevelName	= "_";

		public uint StorageLevels {
			get { return( 2 ); }
		}

		public string KeyForStorageLevel( long blobId, uint level ) {
			var	retValue = cDefaultLevelName;
			var blobStr = blobId.ToString();

			if( blobStr.Length > ( level * 2 )) {
				retValue = blobStr.Substring((int)level * 2, 2 );
			}

			return( retValue );
		}
	}
}
