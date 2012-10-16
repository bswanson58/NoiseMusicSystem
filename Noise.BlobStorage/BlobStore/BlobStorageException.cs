using System;
using System.Globalization;

namespace Noise.BlobStorage.BlobStore {
	public class BlobStorageException : ApplicationException {
		public string	BlobId { get; private set; }
		public string	BlobLocation { get; private set; }

		public BlobStorageException( long blobId, string blobLocation, string message ) :
			base( message ) {
			BlobId = blobId.ToString( CultureInfo.InvariantCulture );
			BlobLocation = blobLocation;
		}

		public BlobStorageException( string blobId, string blobLocation, string message ) :
			base( message ) {
			BlobId = blobId;
			BlobLocation = blobLocation;
		}
	}
}
