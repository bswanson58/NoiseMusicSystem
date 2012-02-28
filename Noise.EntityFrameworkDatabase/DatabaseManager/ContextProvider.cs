using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class ContextProvider : IContextProvider {
		private readonly IBlobStorageManager	mBlobStorageManager;

		public ContextProvider( IBlobStorageManager blobStorageManager ) {
			mBlobStorageManager = blobStorageManager;
		}

		public IBlobStorageManager BlobStorageManager {
			get{ return( mBlobStorageManager ); }
		}

		public IDbContext	CreateContext() {
			return( new NoiseContext());
		}
	}
}
