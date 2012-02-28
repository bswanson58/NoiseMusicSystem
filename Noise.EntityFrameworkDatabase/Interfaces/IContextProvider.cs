using Noise.BlobStorage.BlobStore;

namespace Noise.EntityFrameworkDatabase.Interfaces {
	public interface IContextProvider {
		IBlobStorageManager	BlobStorageManager { get; }

		IDbContext			CreateContext();
	}
}
