using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Interfaces {
	public interface IContextProvider {
		IBlobStorageManager	BlobStorageManager { get; }

		IDbContext			CreateContext();
	}
}
