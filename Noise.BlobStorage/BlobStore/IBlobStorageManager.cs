using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
	public interface IBlobStorageManager {
		bool			Initialize( string rootStoragePath );

		bool			OpenStorage();
		bool			CreateStorage();
		void			CloseStorage();
		void			DeleteStorage();

		bool			IsOpen { get; }
		IBlobStorage	GetStorage();
	}
}
