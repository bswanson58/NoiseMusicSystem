using System.Globalization;
using Noise.Infrastructure.Interfaces;

namespace Noise.Metadata.Support {
	public class MetadataStorageResolver : IMetadataStorageResolver {
		public uint StorageLevels { get; private set; }

		public MetadataStorageResolver() {
			StorageLevels = 1;
		}

		public string KeyForStorageLevel( long blobId, uint level ) {
			return( string.Empty );
		}

		public string KeyForStorageLevel( string blobId, uint level ) {
			var retValue = "";

			if((!string.IsNullOrWhiteSpace( blobId )) &&
			   ( blobId.Length > level )) {
				retValue = blobId[(int)level].ToString( CultureInfo.InvariantCulture );
			}

			return( retValue );
		}
	}
}
