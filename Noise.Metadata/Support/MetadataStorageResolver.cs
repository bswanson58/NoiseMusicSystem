using System.Globalization;

namespace Noise.Metadata.Support {
	public class MetadataStorageResolver : IMetadataStorageResolver {
		public uint StorageLevels { get; private set; }

		public MetadataStorageResolver() {
			StorageLevels = 2;
		}

		public string KeyForStorageLevel( long blobId, uint level ) {
			return( string.Empty );
		}

		public string KeyForStorageLevel( string blobId, uint level ) {
			var retValue = "";

			if(!string.IsNullOrWhiteSpace( blobId )) {
				retValue = level == 0 ? blobId[(int)level].ToString( CultureInfo.InvariantCulture ).ToLower() : blobId;
			}

			return( retValue );
		}
	}
}
