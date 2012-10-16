using System.Globalization;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
	internal class BlobStorageResolver : IBlobStorageResolver {
		private const string	cDefaultLevelName	= "_";

		public uint StorageLevels {
			get { return( 2 ); }
		}

		public string KeyForStorageLevel( string blobId, uint level ) {
			return( string.Empty );
		}

		public string KeyForStorageLevel( long blobId, uint level ) {
			var	retValue = cDefaultLevelName;
			var blobStr = blobId.ToString( CultureInfo.InvariantCulture );

			if( blobStr.Length > ( level * 2 )) {
				retValue = blobStr.Substring((int)level * 2, 2 );
			}

			return( retValue );
		}
	}
}
