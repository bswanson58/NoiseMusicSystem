using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
	public interface IBlobStorageManager {
		bool			Initialize( string rootStoragePath );

		bool			OpenStorage( string storageName );
		bool			CreateStorage( string storageName );
		void			CloseStorage();
		void			DeleteStorage( string storageName );

		bool			IsOpen { get; }
		IBlobStorage	GetStorage();
	}
}
