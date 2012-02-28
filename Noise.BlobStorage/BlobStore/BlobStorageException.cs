using System;

namespace Noise.BlobStorage.BlobStore {
	public class BlobStorageException : ApplicationException {
		public long		BlobId { get; private set; }
		public string	BlobLocation { get; private set; }

		public BlobStorageException( long blobId, string blobLocation, string message ) :
			base( message ) {
			BlobId = blobId;
			BlobLocation = blobLocation;
		}
	}
}
